using System;

namespace CityBudget
{
    public enum Gender
    {
        Male,
        Female
    }

    public class Person
    {
        public Guid Id { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public double Income { get; set; }
        public bool IsEmployed { get; set; }
        public double Happiness { get; set; }

        public Person(int age, Gender gender)
        {
            Id = Guid.NewGuid();
            Age = age;
            Gender = gender;
            Happiness = 50.0;
            IsEmployed = false;
            Income = 0;
        }

        public bool IsWorkingAge()
        {
            return Age >= 18 && Age < 65;
        }
    }
}