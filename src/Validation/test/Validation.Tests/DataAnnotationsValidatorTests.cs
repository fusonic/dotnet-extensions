using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Fusonic.Extensions.Validation.Tests
{
    public class DataAnnotationsValidatorTests
    {
        [Fact]
        public void ValidatesObject()
        {
            var result = DataAnnotationsValidator.Validate(new TestObject());
            Assert.False(result.IsValid);
            Assert.Collection(result.Errors,
                e =>
                {
                    Assert.Equal("Test", e.Key);
                    Assert.Collection(e.Value, er => Assert.Equal("The Test field is required.", er));
                },
                e =>
                {
                    Assert.Equal("Test2", e.Key);
                    Assert.Collection(e.Value, er => Assert.Equal("The field Test2 must be between 5 and 10.", er));
                });
        }

        [Fact]
        public void ValidatesIValidateableObject()
        {
            var result = DataAnnotationsValidator.Validate(new TestValidatableObject());
            Assert.False(result.IsValid);
            Assert.Collection(result.Errors,
                e =>
                {
                    Assert.Equal("", e.Key);
                    Assert.Collection(e.Value, er => Assert.Equal("Error without member", er));
                },
                e =>
                {
                    Assert.Equal("Test", e.Key);
                    Assert.Collection(e.Value,
                        er => Assert.Equal("Error with member", er),
                        er => Assert.Equal("Error with multiple members", er));
                },
                e =>
                {
                    Assert.Equal("Test2", e.Key);
                    Assert.Collection(e.Value, er => Assert.Equal("Error with multiple members", er));
                });
        }

        [Fact]
        public void ValidatesChildObjects()
        {
            var model = new TestObjectWithChild()
            {
                Test = new TestItem()
            };

            var result = DataAnnotationsValidator.Validate(model);
            Assert.False(result.IsValid);
            Assert.Collection(result.Errors,
                e =>
                {
                    Assert.Equal("Test.Test", e.Key);
                    Assert.Collection(e.Value, er => Assert.Equal("The Test field is required.", er));
                });
        }

        [Fact]
        public void ValidatesCollections()
        {
            var model = new TestObjectWithCollection()
            {
                Test = new List<TestItem>()
                {
                    new TestItem(),
                    new TestItem()
                    {
                        Test = "valid"
                    },
                    new TestItem()
                }
            };

            var result = DataAnnotationsValidator.Validate(model);
            Assert.False(result.IsValid);
            Assert.Collection(result.Errors,
                e =>
                {
                    Assert.Equal("Test[0].Test", e.Key);
                    Assert.Collection(e.Value, er => Assert.Equal("The Test field is required.", er));
                },
                e =>
                {
                    Assert.Equal("Test[2].Test", e.Key);
                    Assert.Collection(e.Value, er => Assert.Equal("The Test field is required.", er));
                });
        }

        [Fact]
        public void ValidatesNullCollections()
        {
            var model = new TestObjectWithCollection
            {
                Test = null
            };

            var result = DataAnnotationsValidator.Validate(model);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void PreventsRecursion()
        {
            var model = new RecursiveTestObject
            {
                Test = new RecursiveTestObject()
            };

            var result = DataAnnotationsValidator.Validate(model);
            Assert.False(result.IsValid);
        }

        [Fact]
        // Header dictionary implements IEnumerable but not ICollection
        public void ValidatesHeaderDictionary()
        {
            var dictionary = new
            {
                Dict = new HeaderDictionary()
            };

            var result = DataAnnotationsValidator.Validate(dictionary);
            Assert.True(result.IsValid);
        }


        [Fact]
        public void ReturnsTrueIfValidatingPrimitive()
        {
            var result = DataAnnotationsValidator.Validate("test");
            Assert.True(result.IsValid);
            result = DataAnnotationsValidator.Validate(2134);
            Assert.True(result.IsValid);
        }

        private class HeaderDictionary : ICollection<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>, IEnumerable, IDictionary<string, string>
        {
            private IDictionary<string, string> dictionaryImplementation = new Dictionary<string, string>();

            public HeaderDictionary()
            {
            }

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                return dictionaryImplementation.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable) dictionaryImplementation).GetEnumerator();
            }

            public void Add(KeyValuePair<string, string> item)
            {
                dictionaryImplementation.Add(item);
            }

            public void Clear()
            {
                dictionaryImplementation.Clear();
            }

            public bool Contains(KeyValuePair<string, string> item)
            {
                return dictionaryImplementation.Contains(item);
            }

            public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
            {
                dictionaryImplementation.CopyTo(array, arrayIndex);
            }

            public bool Remove(KeyValuePair<string, string> item)
            {
                return dictionaryImplementation.Remove(item);
            }

            public int Count => dictionaryImplementation.Count;

            public bool IsReadOnly => dictionaryImplementation.IsReadOnly;

            public void Add(string key, string value)
            {
                dictionaryImplementation.Add(key, value);
            }

            public bool ContainsKey(string key)
            {
                return dictionaryImplementation.ContainsKey(key);
            }

            public bool Remove(string key)
            {
                return dictionaryImplementation.Remove(key);
            }

            public bool TryGetValue(string key, out string value)
            {
#nullable disable
                return dictionaryImplementation.TryGetValue(key, out value);
#nullable enable
            }

            public string this[string key]
            {
                get => dictionaryImplementation[key];
                set => dictionaryImplementation[key] = value;
            }

            public ICollection<string> Keys => dictionaryImplementation.Keys;

            public ICollection<string> Values => dictionaryImplementation.Values;
        }
    }
}
