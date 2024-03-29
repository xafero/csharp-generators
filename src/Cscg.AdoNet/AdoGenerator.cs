using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cscg.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Cscg.AdoNet.AdoSource;
using static Cscg.Core.CodeTool;
using F = Cscg.AdoNet.AdoSource;

namespace Cscg.AdoNet
{
    [Generator(LanguageNames.CSharp)]
    public sealed class AdoGenerator : IIncrementalGenerator
    {
        private static readonly string ContextAn = GetAttributeName(ContextAttrName);
        private static readonly string TableAn = GetAttributeName(TableAttrName);
        private static readonly string MappingAn = GetAttributeName(MappingAttrName);
        private static readonly string ColAn = GetAttributeName(ColAttrName);
        private static readonly string KeyAn = GetAttributeName(KeyAttrName);
        private static readonly string ForeignAn = GetAttributeName(ForeignAttrName);
        private static readonly string IncludeAn = GetAttributeName(IncludeAttrName);
        private static readonly string RawSqlAn = GetAttributeName(RawSqlAttrName);

        public void Initialize(IncrementalGeneratorInitializationContext igi)
        {
            var sp = igi.SyntaxProvider;

            var tableAf = GetFullName(LibSpace, TableAn);
            igi.RegisterSourceOutput(sp.ForAttributeWithMetadataName(tableAf, Check, Wrap), Exec);

            var mapAf = GetFullName(LibSpace, MappingAn);
            igi.RegisterSourceOutput(sp.ForAttributeWithMetadataName(mapAf, Check, Wrap), Exec);

            var ctxAf = GetFullName(LibSpace, ContextAn);
            igi.RegisterSourceOutput(sp.ForAttributeWithMetadataName(ctxAf, Check, Wrap), F.Exec);
        }

        private static bool Check(SyntaxNode node, CancellationToken _)
            => node is ClassDeclarationSyntax;

        private static SyntaxWrap Wrap(GeneratorAttributeSyntaxContext ctx, CancellationToken _)
            => ctx.Wrap();

        private static void Exec(SourceProductionContext ctx, SyntaxWrap syntax)
            => ctx.WrapForError(syntax, Exec);

        private static void Exec(ClassDeclarationSyntax cds, string name, CodeWriter code, SyntaxWrap syntax)
        {
            var ccs = syntax.Symbol.FindArgs(simple: true);
            var space = cds.GetParentName() ?? Coding.AutoNamespace;
            code.AddUsings(LibSpace, "System", "System.Linq", "System.Collections.Generic", "Microsoft.Data.Sqlite");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");

            var members = new List<MemberDeclarationSyntax>(cds.Members);
            var innerClasses = new List<string>();
            var innerMembers = members.OfType<ClassDeclarationSyntax>().ToArray();
            var isTree = innerMembers.Length >= 1;

            const string connType = "SqliteConnection";
            const string writType = "SqliteCommand";
            const string readType = "SqliteDataReader";
            var adiType = $"IActiveData<{readType}, {writType}>";
            var adiTypes = new List<string> { adiType };
            if (isTree) adiTypes.Add($"IActiveNested<{name}>");
            code.WriteClassLine(name, interfaces: adiTypes.ToArray());
            var intfCodePos = code.Lines.Count - 1;
            code.AppendLine("{");

            var crea = new CodeWriter();
            crea.AppendLine("public static string CreateTable()");
            crea.AppendLine("{");
            var isMap = ccs.TryGetValue(MappingAn, out _);
            var tableName = isMap ? name : BuildPlural(name);
            if (ccs.TryGetValue($"{TableAn}_Name", out var tbn)) tableName = tbn;
            var table = SqliteSource.Quote(tableName);
            crea.AppendLine("var sql = string.Join(Environment.NewLine, [");
            crea.AppendLine($"@\"CREATE TABLE IF NOT EXISTS \"{table}\" (\",");

            var cus = new CodeWriter();
            var crei = new CodeWriter();
            crei.AppendLine($"public static Table GetTable() => new Table({table}, new []");
            crei.AppendLine("{");

            var deser = new CodeWriter();
            var sqser = new CodeWriter();
            var addB = new CodeWriter();
            var addE = new CodeWriter();
            var savB = new CodeWriter();
            var savE = new CodeWriter();

            var after = new List<string>();
            var inner = new List<string>();
            var mapPk = new Dictionary<string, ITypeSymbol>();
            var lastPk = default(string);
            var lastPkT = default(ITypeSymbol);

            foreach (var innerClass in innerMembers)
            foreach (var member in innerClass.Members)
            {
                if (syntax.GetSymbol(member.Parent) is var icp && !icp.IsAbstract)
                    innerClasses.Add(icp.Name);
                members.Add(member);
            }
            if (isTree) innerClasses.Add(name);

            var includes = new List<(PropertyDeclarationSyntax p, ISymbol s, Dictionary<string, string> a)>();

            foreach (var member in members)
            {
                if (member is PropertyDeclarationSyntax pds)
                {
                    var pps = syntax.GetSymbol(pds);
                    var ppa = pps.FindArgs(simple: true);
                    if (ppa.ContainsKey(IncludeAn))
                        includes.Add((pds, pps, ppa));

                    var pp = syntax.GetInfo(pps);
                    var ppName = ppa.TryGetValue($"{ColAn}_Name", out var tpn) ? tpn : pp.Name;
                    var pName = SqliteSource.Quote(ppName);

                    if (!pp.ReturnType.IsBuiltIn() && pp.ReturnType.TypeKind != TypeKind.Array)
                    {
                        var ppCt = pps.ContainingType;
                        var ppIsWt = ppCt.Name != name;
                        var ppWtN = ppIsWt ? $"x{addB.Lines.Count + 1}" : "entity";
                        if (ppIsWt)
                        {
                            string[] ascWTxt = [$"if (entity is {ppCt} {ppWtN})", "{"];
                            addB.AppendLines(ascWTxt);
                            savB.AppendLines(ascWTxt);
                        }
                        var ppWtNppN = $"{ppWtN}.{ppName}";
                        if (pp.ReturnType.IsTyped(out _, out var prA, out var prList, out _) &&
                            prList && prA.SingleOrDefault() is var prAs)
                        {
                            addE.AppendLine($"foreach (var item in {ppWtNppN} ?? [])");
                            addE.AppendLine("{");
                            addE.AppendLine($"{prAs.Name}DbSet.iAdd(ctx, item);");
                            addE.AppendLine("}");

                            savE.AppendLine($"foreach (var item in {ppWtNppN} ?? [])");
                            savE.AppendLine("{");
                            savE.AppendLine($"{prAs.Name}DbSet.iSave(ctx, conn, item);");
                            savE.AppendLine("}");
                        }
                        else
                        {
                            addB.AppendLine($"if ({ppWtNppN} != default)");
                            addB.AppendLine("{");
                            addB.AppendLine($"{pp.ReturnType.Name}DbSet.iAdd(ctx, {ppWtNppN});");
                            addB.AppendLine("}");

                            savB.AppendLine($"if ({ppWtNppN} != default)");
                            savB.AppendLine("{");
                            savB.AppendLine($"{ppWtNppN} = {pp.ReturnType.Name}DbSet.iSave(ctx, conn, {ppWtNppN});");
                            savB.AppendLine($"{ppWtNppN}Id = {ppWtNppN}.Id;");
                            savB.AppendLine("}");
                        }
                        if (ppIsWt)
                        {
                            addB.AppendLine("}");
                            savB.AppendLine("}");
                        }
                    }

                    if (!ppa.ContainsKey(ColAn))
                        continue;
                    var isPrimary = ppa.TryGetValue(KeyAn, out _);
                    var pk = !isPrimary
                        ? null
                        : SqliteSource.Quote($"PK_{tableName.Trim('"')}");
                    if (pk != null)
                    {
                        lastPk = ppName;
                        lastPkT = pp.ReturnType;
                    }

                    ppa.TryGetValue($"{ForeignAn}_Table", out var ft);
                    ppa.TryGetValue($"{ForeignAn}_Column", out var fc);
                    if (ppa.TryGetValue(ForeignAn, out _))
                    {
                        ppa.TryGetValue($"{ForeignAn}_Unique", out var fu);
                        ppa.TryGetValue($"{ForeignAn}_NoCascade", out var fd);
                        var u = fu == "true";
                        var d = fd == "true";
                        var (fi, fo) = SqliteSource.GetForeign(tableName, ppName, ft, fc, u, d);
                        inner.AddRange(fi);
                        if (isMap)
                            mapPk.Add(ppName, pp.ReturnType);
                        after.AddRange(fo);
                    }

                    var forceNull = mapPk.ContainsKey(ppName) ? false : default(bool?);
                    var (pType, pCond) = SqliteSource.GetType(pp.ReturnType, pk, forceNull);
                    crea.AppendLine($"@\"    \"{pName}\" {pType} {pCond},\",");

                    var creiP = Typing.ToStr(isPrimary);
                    var creiT = Typing.ToStr(ft);
                    var creiC = Typing.ToStr(fc);
                    crei.AppendLine($"   new Column({pName}, {creiP}, {creiT}, {creiC}),");

                    var pno = "this";
                    var pNameSuf = string.Empty;
                    var pNamePre = string.Empty;
                    var pDef = pp.ReturnType;
                    var ppReading = $"r.IsDBNull(i) ? default({pDef}) : {SqliteSource.GetRead(pp)}";
                    if (isTree && syntax.GetSymbol(pds.Parent) is var tpp && tpp.Name != name)
                    {
                        pno = tpp.Name.ToSnake();
                        var pNameTmp = $"(Tmp ?? this) is {tpp.Name} {pno}";
                        pNameSuf = $" && {pNameTmp}";
                        pNamePre = $"{pNameTmp} && ";
                    }

                    deser.AppendLine($"if (key == {pName}{pNameSuf})");
                    deser.AppendLine("{");
                    deser.AppendLine($"{pno}.{pp.Name} = {ppReading};");
                    if (isTree && pp.Name == DiscriminatorFld)
                    {
                        deser.AppendLine($"if ({pno}.{pp.Name} != default)");
                        deser.AppendLine("{");
                        var y = string.Join(", ", innerClasses.Select(icc => $"\"{icc}\" => new {icc}()"));
                        deser.AppendLine($"this.Tmp = {pno}.{pp.Name} switch {{ {y} }};");
                        deser.AppendLine($"this.Tmp.{lastPk} = this.{lastPk};");
                        deser.AppendLine($"this.Tmp.{DiscriminatorFld} = this.{DiscriminatorFld};");
                        deser.AppendLine("}");
                    }
                    deser.AppendLine("return;");
                    deser.AppendLine("}");

                    if (isTree && pp.Name == DiscriminatorFld)
                    {
                        var x = string.Join(", ", innerClasses.Select(icc => $"{icc} => \"{icc}\""));
                        sqser.AppendLine($"{pno}.{pp.Name} = this switch {{ {x} }};");
                    }
                    var pNamePm = $"\"@p{pName.TrimStart('"')}";
                    sqser.AppendLine($"if ({pNamePre}{pno}.{pp.Name} != default)");
                    sqser.AppendLine("{");
                    sqser.AppendLine($"w.Parameters.AddWithValue({pNamePm}, {SqliteSource.GetWrite(pp, pno)});");
                    sqser.AppendLine("}");
                }
                else if (member is MethodDeclarationSyntax mds)
                {
                    var mms = (IMethodSymbol)syntax.GetSymbol(mds);
                    var mma = mms.FindArgs(simple: true);
                    if (mma.ContainsKey(RawSqlAn))
                    {
                        mma.TryGetValue($"{RawSqlAn}_Mapping", out var mmm);
                        var mmName = mms.Name;
                        var mmPar = mms.Parameters;
                        var mmPars = string.Join(", ", mmPar.Select(p => $"{p.Type} {p.Name}"));
                        var mmx = Maps.SplitMap(mmm).ToDict();
                        var mmt = $"({string.Join(", ", mmx.Select(q => $"{q.Value} {q.Key}"))})";
                        cus.AppendLine();
                        cus.AppendLine($"public {mmt}[] {mmName}({mmPars})");
                        cus.AppendLine("{");
                        cus.AppendLine("using var cmd = Conn.CreateCommand();");
                        var mmParLen = mmPar.Length;
                        mmPars = string.Join(", ", Enumerable.Range(0, mmParLen).Select(_ => "default"));
                        cus.AppendLine($"cmd.CommandText = {name}.{mmName}({mmPars});");
                        foreach (var prm in mmPar)
                        {
                            var mp = prm.Name;
                            cus.AppendLine($"cmd.Parameters.AddWithValue(\"@{mp}\", ((object){mp}) ?? DBNull.Value);");
                        }
                        cus.AppendLine("var prefix = new Dictionary<string, string>");
                        cus.AppendLine("{");
                        cus.AppendLine(string.Join(", ", mmx.GroupBy(w => w.Value).Select(w =>
                            $"[\"{w.Key}\"] = \"{string.Join("|", w.Select(q => q.Key))}\"")));
                        cus.AppendLine("};");
                        cus.AppendLine("using var reader = cmd.ExecuteReader();");
                        var tblRea = string.Join(", ", mmx.Values);
                        cus.AppendLine($"var res = reader.ReadData<{tblRea}, {readType}>(prefix);");
                        cus.AppendLine("return res.Select(x => x.Value).ToArray();");
                        cus.AppendLine("}");
                    }
                }
            }

            if (isMap)
            {
                inner.Insert(0, SqliteSource.GetMapKey(tableName, mapPk.Keys));
            }

            foreach (var item in inner)
            {
                crea.AppendLine(item);
            }

            crei.ModifyLast(l => l.Replace(",\",", "\","));
            crei.AppendLine("});");

            crea.ModifyLast(l => l.Replace(",\",", "\","));
            crea.AppendLine("\");\"");
            if (after.Any())
            {
                crea.ModifyLast(l => l + ",");
                crea.AppendLines(["\"\",", "\"\","]);
                crea.AppendLines(after);
                crea.ModifyLast(l => l.Replace(";\",", ";\""));
            }
            crea.AppendLine("]);");
            crea.AppendLine("return sql;");
            crea.AppendLine("}");

            var sel = new CodeWriter();
            if (isTree)
            {
                sel.AppendLine("object IActiveNested.Inner => Inner;");
                sel.AppendLine($"public {name} Inner => Tmp;");
                sel.AppendLine($"protected {name} Tmp {{ get; private set; }}");
                sel.AppendLine();
            }
            sel.AppendLine($"public void ReadSql({readType} r, string key, int i)");
            sel.AppendLine("{");
            sel.AppendLines(deser);
            sel.AppendLine("}");
            sel.AppendLine();
            sel.AppendLine($"public void WriteSql({writType} w)");
            sel.AppendLine("{");
            sel.AppendLines(sqser);
            sel.AppendLine("}");

            var asx = new CodeWriter();
            asx.AppendLine($"internal static void iAdd(DbContext ctx, {name} entity)");
            asx.AppendLine("{");
            asx.AppendLines(["if (entity == default)", "{", "return;", "}"]);
            asx.AppendLines(addB);
            asx.AppendLine("ctx.Enqueue(entity);");
            asx.AppendLines(addE);
            asx.AppendLine("}");
            asx.AppendLine();
            asx.AppendLine($"public override void Add({name} entity)");
            asx.AppendLine("{");
            asx.AppendLine("iAdd(Context, entity);");
            asx.AppendLine("}");
            asx.AppendLine();
            asx.AppendLine($"public override void Add(object entity) => Add(({name})entity);");
            asx.AppendLine();
            asx.AppendLine($"internal static {name} iSave(DbContext ctx, {connType} conn, {name} entity)");
            asx.AppendLine("{");
            asx.AppendLines(savB);
            if (lastPk == null)
                asx.AppendLine("iInsert(conn, entity);");
            else
                asx.AppendLine($"(entity.{lastPk}, entity) = ctx.Cache<{name}, {lastPkT}>(entity, e => iInsert(conn, e));");
            asx.AppendLines(savE);
            asx.AppendLine("return entity;");
            asx.AppendLine("}");
            asx.AppendLine();
            asx.AppendLine($"public override void Save({name} entity)");
            asx.AppendLine("{");
            asx.AppendLine("iSave(Context, Conn, entity);");
            asx.AppendLine("}");
            asx.AppendLine();
            asx.AppendLine($"public override void Save(object entity) => Save(({name})entity);");

            var sam = new CodeWriter();
            sam.AppendLine($"public {name}[] FindSame(params Action<{name}>[] func)");
            sam.AppendLine("{");
            sam.AppendLine("using var cmd = Conn.CreateCommand();");
            sam.AppendLine($"var sample = new {name}();");
            sam.AppendLine("Array.ForEach(func, f => f(sample));");
            sam.AppendLine("sample.WriteSql(cmd);");
            sam.AppendLine($@"cmd.CommandText = cmd.GetColumns().CreateSelect({table});");
            sam.AppendLine("using var reader = cmd.ExecuteReader();");
            sam.AppendLine($"return reader.ReadData<{name}, {readType}>().ToArray();");
            sam.AppendLine("}");

            var del = new CodeWriter();
            var upd = new CodeWriter();
            var ins = new CodeWriter();
            var fin = new CodeWriter();
            var lst = new CodeWriter();
            if (!string.IsNullOrWhiteSpace(lastPk))
            {
                code.Lines.ModifyLast(intfCodePos, f => $"{f}, IHasId<{lastPkT}>");
                if (lastPk != "Id")
                    code.Lines.Insert(intfCodePos + 2, $"\t\tpublic {lastPkT} Id => {lastPk};{Texts.NewLine}");

                lst.AppendLine($"public {name}[] List(int offset = 0, int limit = 10)");
                lst.AppendLine("{");
                lst.AppendLine("using var cmd = Conn.CreateCommand();");
                lst.AppendLine($@"cmd.CommandText = @""SELECT * FROM ""{table}"" ORDER BY {lastPk} LIMIT @p0 OFFSET @p1;"";");
                lst.AppendLine($@"cmd.Parameters.AddWithValue(""@p0"", limit);");
                lst.AppendLine($@"cmd.Parameters.AddWithValue(""@p1"", offset);");
                lst.AppendLine("using var reader = cmd.ExecuteReader();");
                lst.AppendLine($"return reader.ReadData<{name}, {readType}>().ToArray();");
                lst.AppendLine("}");
                lst.AppendLine();
                lst.AppendLine($"public {name}[] Latest({lastPkT} lastId = default, int limit = 10)");
                lst.AppendLine("{");
                lst.AppendLine("using var cmd = Conn.CreateCommand();");
                lst.AppendLine($"var tmp = lastId == default ? \" \" : \" WHERE {lastPk} < @p1 \";");
                lst.AppendLine($@"cmd.CommandText = $@""SELECT * FROM ""{table}""{{tmp}}ORDER BY {lastPk} DESC LIMIT @p0;"";");
                lst.AppendLine($@"cmd.Parameters.AddWithValue(""@p0"", limit);");
                lst.AppendLine($@"cmd.Parameters.AddWithValue(""@p1"", lastId);");
                lst.AppendLine("using var reader = cmd.ExecuteReader();");
                lst.AppendLine($"return reader.ReadData<{name}, {readType}>().ToArray();");
                lst.AppendLine("}");

                fin.AppendLine($"public {name} Find({lastPkT} id)");
                fin.AppendLine("{");
                fin.AppendLine("using var cmd = Conn.CreateCommand();");
                fin.AppendLine($@"cmd.CommandText = @""SELECT * FROM ""{table}"" WHERE {lastPk} = @p0;"";");
                fin.AppendLine(@"cmd.Parameters.AddWithValue(""@p0"", id);");
                fin.AppendLine("using var reader = cmd.ExecuteReader();");
                fin.AppendLine($"return reader.ReadData<{name}, {readType}>().FirstOrDefault();");
                fin.AppendLine("}");

                if (includes.Any())
                {
                    fin.AppendLine();
                    var includeTypes = new List<string> { name };
                    foreach (var include in includes)
                    {
                        includeTypes.Add(include.s.Name);
                        if (include.a.TryGetValue($"{IncludeAn}_Add", out var iaa))
                            foreach (var inclType in Typing.SplitTypeOf(iaa))
                                includeTypes.Add(Typing.SplitType(inclType).name);
                    }
                    fin.AppendLine($"public {name} FindInclude({lastPkT} id)");
                    fin.AppendLine("{");
                    fin.AppendLine("using var cmd = Conn.CreateCommand();");
                    var tblInd = string.Join(", ", includeTypes.Select(it => $"{it}.GetTable()"));
                    fin.AppendLine($"Table[] tables = {{ {tblInd} }};");
                    fin.AppendLine("var prefix = tables.GetTablePrefixes();");
                    fin.AppendLine($@"cmd.CommandText = tables.CreateJoin(prefix, ""0"");");
                    fin.AppendLine($"cmd.Parameters.AddWithValue(\"@p0\", id);");
                    fin.AppendLine("using var reader = cmd.ExecuteReader();");
                    var tblRea = string.Join(", ", includeTypes);
                    fin.AppendLine($"var res = reader.ReadData<{tblRea}, {readType}>(prefix).FirstOrDefault();");
                    fin.AppendLine("if (res == null) return null;");
                    fin.AppendLine($"var item = res.Value.Item1;");
                    var idx = 1;
                    var tblItm = "item";
                    foreach (var include in includeTypes.Skip(1))
                    {
                        var curr = $"{tblItm}.{include}";
                        fin.AppendLine($"{curr} = res.Value.Item{++idx};");
                        tblItm = curr;
                    }
                    fin.AppendLine("return item;");
                    fin.AppendLine("}");

                    lst.AppendLine();
                    lst.AppendLine($"public {name}[] ListInclude(int limit = 100, int offset = 0)");
                    lst.AppendLine("{");
                    lst.AppendLine("using var cmd = Conn.CreateCommand();");
                    lst.AppendLine($"Table[] tables = {{ {tblInd} }};");
                    lst.AppendLine("var prefix = tables.GetTablePrefixes();");
                    lst.AppendLine($@"cmd.CommandText = tables.CreateJoin(prefix, limited: true);");
                    lst.AppendLine($@"cmd.Parameters.AddWithValue(""@p0"", limit);");
                    lst.AppendLine($@"cmd.Parameters.AddWithValue(""@p1"", offset);");
                    lst.AppendLine("using var reader = cmd.ExecuteReader();");
                    lst.AppendLine($"var res = reader.ReadData<{tblRea}, {readType}>(prefix).Select(x =>");
                    lst.AppendLine("{");
                    lst.AppendLine("var item = x.Value.Item1;");
                    idx = 1;
                    tblItm = "item";
                    foreach (var include in includeTypes.Skip(1))
                    {
                        var curr = $"{tblItm}.{include}";
                        lst.AppendLine($"{curr} = x.Value.Item{++idx};");
                        tblItm = curr;
                    }
                    lst.AppendLine("return item;");
                    lst.AppendLine("}).ToArray();");
                    lst.AppendLine("return res;");
                    lst.AppendLine("}");
                }

                ins.AppendLine($"internal static {lastPkT} iInsert({connType} conn, {name} entity)");
                ins.AppendLine("{");
                ins.AppendLine("using var cmd = conn.CreateCommand();");
                ins.AppendLine("entity.WriteSql(cmd);");
                ins.AppendLine($@"cmd.CommandText = cmd.GetColumns().CreateInsert({table}, ""{lastPk}"");");
                ins.AppendLine($"return cmd.ExecuteScalar().ConvertTo<{lastPkT}>();");
                ins.AppendLine("}");
                ins.AppendLine();
                ins.AppendLine($"public {lastPkT} Insert({name} entity)");
                ins.AppendLine("{");
                ins.AppendLine("return iInsert(Conn, entity);");
                ins.AppendLine("}");

                upd.AppendLine($"public bool Update({name} entity)");
                upd.AppendLine("{");
                upd.AppendLine("using var cmd = Conn.CreateCommand();");
                upd.AppendLine("entity.WriteSql(cmd);");
                upd.AppendLine($@"cmd.CommandText = cmd.GetColumns().CreateUpdate({table}, ""{lastPk}"");");
                upd.AppendLine("return cmd.ExecuteNonQuery() == 1;");
                upd.AppendLine("}");

                del.AppendLine($"public bool Delete({name} entity)");
                del.AppendLine("{");
                del.AppendLine("using var cmd = Conn.CreateCommand();");
                del.AppendLine($@"cmd.CommandText = @""DELETE FROM ""{table}"" WHERE {lastPk} = @p0;"";");
                del.AppendLine($@"cmd.Parameters.AddWithValue(""@p0"", entity.{lastPk});");
                del.AppendLine("return cmd.ExecuteNonQuery() == 1;");
                del.AppendLine("}");
            }

            if (isMap && mapPk.Count >= 1)
            {
                var mapIdK = '(' + string.Join(", ", mapPk.Select(m => m.Value)) + ')';
                var mapIdV = string.Join(", ", mapPk.Select(m => m.Key));
                code.Lines.ModifyLast(intfCodePos, f => $"{f}, IHasId<{mapIdK}>");
                code.Lines.Insert(intfCodePos + 2, $"\t\tpublic {mapIdK} Id => ({mapIdV});{Texts.NewLine}");

                ins.AppendLine($"internal static bool iInsert({connType} conn, {name} entity)");
                ins.AppendLine("{");
                ins.AppendLine("using var cmd = conn.CreateCommand();");
                ins.AppendLine("entity.WriteSql(cmd);");
                ins.AppendLine($@"cmd.CommandText = cmd.GetColumns().CreateInsert({table});");
                ins.AppendLine($"return cmd.ExecuteNonQuery() == 1;");
                ins.AppendLine("}");
                ins.AppendLine();
                ins.AppendLine($"public bool Insert({name} entity)");
                ins.AppendLine("{");
                ins.AppendLine("return iInsert(Conn, entity);");
                ins.AppendLine("}");

                del.AppendLine($"public bool Delete({name} entity)");
                del.AppendLine("{");
                del.AppendLine("using var cmd = Conn.CreateCommand();");
                del.AppendLine("entity.WriteSql(cmd);");
                del.AppendLine($@"cmd.CommandText = cmd.GetColumns().CreateDelete({table});");
                del.AppendLine("return cmd.ExecuteNonQuery() == 1;");
                del.AppendLine("}");

                foreach (var oneMap in mapPk)
                {
                    var omp = oneMap.Key.ToSnake();
                    var omt = oneMap.Value.ToTrimDisplay();
                    fin.AppendLine($"public {name}[] FindBy{oneMap.Key}({omt} {omp}) => " +
                                   $"FindSame(x => x.{oneMap.Key} = {omp});");
                }
            }

            var body = new CodeWriter();
            body.AppendLines(crea);
            body.AppendLine();
            body.AppendLines(crei);
            body.AppendLine();
            body.AppendLines(sel);

            code.AppendLines(body);
            code.AppendLine("}");
            code.AppendLine();
            code.AppendLines(NewSet(name, connType, sam, fin, lst, upd, ins, del, cus, asx));
            code.AppendLine("}");
        }
    }
}