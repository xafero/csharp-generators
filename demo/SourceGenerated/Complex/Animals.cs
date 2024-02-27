using System;
using System.Collections.Generic;

namespace SourceGenerated.Complex
{
    public static class Zoos
    {
        public static Zoo CreateTestSample()
        {
            var zoo = new Zoo();
            var lion = new Lion
            {
                Name = "Simba", Age = 3,
                Diet = new DietInfo { FoodType = "Meat", QuantityPerDay = 5 },
                Classification = AnimalClassification.Fish
            };
            zoo.Animals.Add(lion);
            zoo.AnimalDiets[lion.Name] = lion.Diet;
            var tiger = new Tiger
            {
                Name = "Louie", Age = 6,
                NickName = "Aggro",
                Classification = AnimalClassification.Amphibian
            };
            zoo.Animals.Add(tiger);
            zoo.AnimalDiets[tiger.Name] = new DietInfo { FoodType = "Human", QuantityPerDay = 1 };
            zoo.Opening = new ZooEvent
            {
                Duration = TimeSpan.FromHours(4.3),
                EventDate = DateTimeOffset.UtcNow.AddYears(-21),
                Guests = 39,
                Money = 29.99
            };
            zoo.Favorite = new Cat
            {
                Name = "Purry", Age = 1, Classification = AnimalClassification.Bird, Cuteness = 125
            };
            zoo.AnimalDiets[zoo.Favorite.Name] = new ExDietInfo
            {
                FoodType = "Special", Premium = true
            };
            return zoo;
        }
    }

    public class ZooEvent
    {
        public int? Guests { get; set; }
        public double? Money { get; set; }
        public DateTimeOffset EventDate { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class Zoo
    {
        public List<IAnimal> Animals { get; set; } = new();
        public Animal Favorite { get; set; }
        public Dictionary<string, DietInfo> AnimalDiets { get; set; } = new();
        public ZooEvent Opening { get; set; }
    }

    public class Cat : Animal
    {
        public int Cuteness { get; set; }
    }

    public class Tiger : Animal
    {
        public string NickName { get; set; }
    }

    public class Lion : Animal
    {
        public DietInfo Diet { get; set; }
    }

    public class DietInfo
    {
        public string FoodType { get; set; }
        public int QuantityPerDay { get; set; }
    }

    public class ExDietInfo : DietInfo
    {
        public bool Premium { get; set; }
    }

    public abstract class Animal : IAnimal
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public AnimalClassification Classification { get; set; }
    }

    public interface IAnimal
    {
        string Name { get; }
    }

    public enum AnimalClassification
    {
        Mammal,
        Bird,
        Reptile,
        Amphibian,
        Fish,
        Invertebrate
    }
}