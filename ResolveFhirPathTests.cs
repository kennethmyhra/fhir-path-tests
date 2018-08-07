using Hl7.Fhir.ElementModel;
using Hl7.Fhir.FhirPath;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.FhirPath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FhirPathTests
{
    [TestClass]
    public class ResolveFhirPathTests
    {
        Bundle _bundle;
        IElementNavigator _bundleNavigator;

        [TestInitialize]
        public void SetupSource()
        {
            ElementNavFhirExtensions.PrepareFhirSymbolTableFunctions();
            string bundleXml = File.ReadAllText("TestData\\bundle-contained-references.xml");

            _bundle = (new FhirXmlParser()).Parse<Bundle>(bundleXml);
            _bundleNavigator = new ScopedNavigator(new PocoNavigator(_bundle));
        }


        [TestMethod]
        public void ResolveQuery()
        {
            string statement = "Bundle.entry.where(fullUrl = 'http://example.org/fhir/Patient/e')"
                         + ".resource.id";
            var result = _bundleNavigator.Select(statement);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("e", result.First().Value);

            statement = "Bundle.entry.where(fullUrl = 'http://example.org/fhir/Patient/e')"
                + ".resource.managingOrganization.reference";
            result = _bundleNavigator.Select(statement);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("#orgY", result.First().Value);

            statement = "Bundle.entry.where(fullUrl = 'http://example.org/fhir/Patient/e')"
                + ".resource.managingOrganization.resolve().id";
            result = _bundleNavigator.Select(statement);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("orgY", result.First().Value);
        }

        [TestMethod]
        public void ResolvePersonWithTwoNameSections()
        {
            FhirPathCompiler compiler = new FhirPathCompiler(CustomFhirPathFunctions.GetSymbolTable());

            Person person = new Person
            {
                Id = "1",
                Identifier = new List<Identifier>
                {
                    new Identifier
                    {
                        System = "2.16.578.1.12.4.1.4.1",
                        Value = "16027512345"
                    }
                },
                Name = new List<HumanName>
                {
                    new HumanName
                    {
                        Use = HumanName.NameUse.Official,
                        Given = new[] { "Kenneth", },
                        Family = "Myhra"
                    },
                    new HumanName
                    {
                        Use = HumanName.NameUse.Old,
                        Given = new[] { "Kenneth" },
                        Family = "AnnetEtternavn"
                    }
                },
                Telecom = new List<ContactPoint>
                {
                    new ContactPoint
                    {
                        Use = ContactPoint.ContactPointUse.Mobile,
                        System = ContactPoint.ContactPointSystem.Phone,
                        Value = "93228677"
                    },
                    new ContactPoint
                    {
                        Use = ContactPoint.ContactPointUse.Home,
                        System = ContactPoint.ContactPointSystem.Phone,
                        Value = "93228677"
                    },
                    new ContactPoint
                    {
                        Use = ContactPoint.ContactPointUse.Home,
                        System = ContactPoint.ContactPointSystem.Email,
                        Value = "kennethmyhra@gmail.com"
                    }
                },
                Address = new List<Address>
                {
                    new Address
                    {
                        Use = Address.AddressUse.Home,
                        Line = new [] { "Snipemyrveien 16" },
                        PostalCode = "1273",
                        City = "Oslo",
                        Country = "Norway"
                    }
                }
            };

            var personNavigator = new ScopedNavigator(new PocoNavigator(person));

            // Retrieve official name instance, 
            // then concatenate given and family separating them by a white space
            IEnumerable<IElementNavigator> result = personNavigator.Select("name.where(use = 'official').select(given & ' ' & family)", compiler: compiler);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Kenneth Myhra", result.Single().Value);

            // Retrieve the distinct values of name and given in a collection for all name instances
            result = personNavigator.Select("(name.given | name.family)", compiler: compiler);
            Assert.AreEqual(3, result.Count());
            Trace.WriteLine(result.ToString());
            Assert.AreEqual("Kenneth", result.First().Value);
            Assert.AreEqual("Myhra", result.Skip(1).First().Value);
            Assert.AreEqual("AnnetEtternavn", result.Skip(2).First().Value);

            // Find identifier that represent FNR
            result = personNavigator.Select("Person.identifier.where(system = '2.16.578.1.12.4.1.4.1').value", compiler: compiler);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("16027512345", result.First().Value);

            // Find patient with family equal to 'Myhra'
            result = personNavigator.Select("Person.name.where(family = 'Myhra').family", compiler: compiler);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Myhra", result.First().Value);

            // Retrieve all name instances
            result = personNavigator.Select("Person.name.select(given.join(' ') & ' ' & family)", compiler: compiler);
            Assert.AreEqual(2, result.Count());
            var name = result.First().Value;
            Assert.AreEqual("Kenneth Myhra", name);
            name = result.Skip(1).First().Value;
            Assert.AreEqual("Kenneth AnnetEtternavn", name);

            // Retrieve first name instance
            result = personNavigator.Select("Person.name.select(given & ' ' & family).first()", compiler: compiler);
            Assert.AreEqual(1, result.Count());
            name = result.SingleOrDefault().Value;
            Assert.IsNotNull(name);
            Assert.AreEqual("Kenneth Myhra", name);

            // Retrieve last name instance
            result = personNavigator.Select("Person.name.select(given & ' ' & family).last()", compiler: compiler);
            Assert.AreEqual(1, result.Count());
            name = result.SingleOrDefault().Value;
            Assert.IsNotNull(name);
            Assert.AreEqual("Kenneth AnnetEtternavn", name);

            // Norwegian First name standard
            result = personNavigator.Select("Person.name.where(use = 'official').select(iif(given.count() > 1, given.take(count()-1), given).join(' '))", compiler: compiler);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Kenneth", result.Single().Value);

            // Norwegian middle name standard
            result = personNavigator.Select("Person.name.where(use = 'official').select(iif(given.count() > 1, given.last(), ''))", compiler: compiler);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("", result.Single().Value);

            // Family name / surname
            result = personNavigator.Select("Person.name.where(use = 'official').family", compiler: compiler);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Myhra", result.Single().Value);

            // Full name
            result = personNavigator.Select("Person.name.where(use = 'official').select(given.join(' ') & ' ' & family)", compiler: compiler);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Kenneth Myhra", result.Single().Value);

            // Phone number
            result = personNavigator.Select("Person.telecom.where(use = 'home' and system = 'phone').value");
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("93228677", result.Single().Value);

            // E-mail
            result = personNavigator.Select("Person.telecom.where(use = 'home' and system = 'email').value");
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("kennethmyhra@gmail.com", result.Single().Value);

            // Adresselinje 1
            result = personNavigator.Select("Person.address.where(use = 'home').line.first()");
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Snipemyrveien 16", result.Single().Value);

            // Adresselinje 2
            result = personNavigator.Select("Person.address.where(use = 'home').line.skip(1).first()");
            Assert.AreEqual(0, result.Count());

            // Adresselinje 3
            result = personNavigator.Select("Person.address.where(use = 'home').line.skip(2).first()");
            Assert.AreEqual(0, result.Count());

            // Postnummer
            result = personNavigator.Select("Person.address.where(use = 'home').postalCode");
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("1273", result.Single().Value);

            // Poststed
            result = personNavigator.Select("Person.address.where(use = 'home').city");
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Oslo", result.Single().Value);

            // Land
            result = personNavigator.Select("Person.address.where(use = 'home').country");
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Norway", result.Single().Value);

            // --------------

            // person2 has multiple given names.
            Person person2 = new Person
            {
                Id = "1",
                Identifier = new List<Identifier>
                {
                    new Identifier
                    {
                        System = "2.16.578.1.12.4.1.4.1",
                        Value = "16027512345"
                    }
                },
                Name = new List<HumanName>
                {
                    new HumanName
                    {
                        Use = HumanName.NameUse.Official,
                        Given = new[] { "Lars", "Kristoffer", "Ulstein" },
                        Family = "Jørgensen"
                    }
                }
            };
            
            var personNavigator2 = new ScopedNavigator(new PocoNavigator(person2));

            // Norwegian First name standard
            result = personNavigator2.Select("Person.name.where(use = 'official').select(iif(given.count() > 1, given.take(count()-1), given).join(' '))", compiler: compiler);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Lars Kristoffer", result.Single().Value);

            // Norwegian middle name standard
            result = personNavigator2.Select("Person.name.where(use = 'official').select(iif(given.count() > 1, given.last(), ''))", compiler: compiler);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Ulstein", result.Single().Value);

            // Family name / surname
            result = personNavigator2.Select("Person.name.where(use = 'official').family", compiler: compiler);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Jørgensen", result.Single().Value);

            // Full name
            result = personNavigator2.Select("Person.name.where(use = 'official').select(given.join(' ') & ' ' & family)", compiler: compiler);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Lars Kristoffer Ulstein Jørgensen", result.Single().Value);

        }
    }
}
