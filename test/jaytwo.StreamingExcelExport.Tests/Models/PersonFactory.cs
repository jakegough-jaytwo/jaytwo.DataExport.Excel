using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Bogus;

namespace jaytwo.StreamingExcelExport.Tests.Models;

public static class PersonFactory
{
    private static Faker<Person> Faker { get; } = new Faker<Person>()
        .RuleFor(p => p.Name, f => f.Name.FullName())
        .RuleFor(p => p.Age, f => f.Random.Int(18, 99))
        .RuleFor(p => p.Biography, f => f.Lorem.Paragraphs(5))
        .RuleFor(p => p.Lipsum, f => f.Lorem.Paragraphs(5));

    public static IEnumerable<Person> GeneratePeople(int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return Faker.Generate();
        }
    }

    public static async IAsyncEnumerable<Person> GeneratePeopleAsync(int count, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return Faker.Generate();
            await Task.Yield();
        }
    }
}
