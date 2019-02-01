﻿using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Example.FluentDbTools.Database.Entities;
using FluentAssertions;
using FluentDbTools.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Example.FluentDbTools.Database
{
    public static class DbExampleExecutor
    {
        public static async Task ExecuteDbExample(
            SupportedDatabaseTypes databaseType,
            Dictionary<string, string> overrideConfig = null)
        {
            var provider = DbExampleBuilder.BuildDbExample(
                databaseType,
                overrideConfig);

            var persons = CreatePersons(10).ToArray();
            
            using (var scope = provider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetService<IPersonRepository>();

                foreach (var person in persons)
                {
                    await repository.InsertPerson(person);
                }
                
                (await repository.SelectPerson(persons.First().Id)).Should().BeEquivalentTo(persons.First());

                var subsetPersons = persons.Take(persons.Length / 2).ToArray();
                var selectedPersons = (await repository.SelectPersons(subsetPersons.Select(x => x.Id).ToArray())).Select(x => x.Id).ToArray();
                selectedPersons.Length.Should().Be(subsetPersons.Length);
                selectedPersons.Should().Contain(subsetPersons.Select(x => x.Id));

                persons.First().Alive = false;
                persons.First().Username = "New Name";
                await repository.UpdatePerson(persons.First());
                (await repository.SelectPerson(persons.First().Id)).Should().BeEquivalentTo(persons.First());

                await repository.DeletePerson(persons.First().Id);
                (await repository.SelectPersons(persons.Select(x => x.Id).ToArray())).Should().NotContain(persons.First());
            }
            
        }

        private static IEnumerable<Person> CreatePersons(int nPersons)
        {
            var persons = new List<Person>();
            for (var i = 0; i < nPersons; i++)
            {
                var person = new Person {SequenceNumber = i + 1};
                persons.Add(person);
            }

            return persons;
        }
    }
}