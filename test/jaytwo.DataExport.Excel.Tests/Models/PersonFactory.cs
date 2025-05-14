using System.Collections.Generic;
using System.Linq;
using Bogus;

namespace jaytwo.DataExport.Excel.Tests.Models;

public static class PersonFactory
{
    private static Faker<Person> PersonFaker { get; } = new Faker<Person>()
        .RuleFor(p => p.Name, f => f.Name.FullName())
        .RuleFor(p => p.BirthDate, f => f.Date.Between(new System.DateTime(1950, 1, 1), new System.DateTime(2000, 12, 31)))
        .RuleFor(p => p.Biography, f => f.Lorem.Paragraphs(5))
        .RuleFor(p => p.Lipsum, f => f.Lorem.Paragraphs(5))
        .RuleFor(p => p.Age, f => f.Random.Int(18, 99));

    private static Faker<Pet> PetFaker { get; } = new Faker<Pet>()
        .RuleFor(p => p.Name, f => f.Name.FullName())
        .RuleFor(p => p.Animal, f => f.PickRandom("dog", "cat", "bird", "hamster", "rabbit", "fish", "guinea pig", "lizard", "ferret"));

    public static IEnumerable<Person> GeneratePeople(int count)
        => Enumerable.Range(1, count).Select(x => PersonFaker.Generate());

    public static IEnumerable<Pet> GeneratePets(int count)
        => Enumerable.Range(1, count).Select(x => PetFaker.Generate());
}
