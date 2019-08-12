using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace Fusonic.Extensions.Validation.Tests
{
    public class TestObject 
    {
        [Required]
        public string Test { get; set; }

        [Range(5, 10)]
        public int Test2 { get; set; }
    }

    public class TestItem
    {
        [Required]
        public string Test { get; set; }
    }

    public class TestObjectWithChild
    {
        public TestItem Test { get; set; }
    }

    public class TestObjectWithCollection
    {
        public List<TestItem> Test { get; set; }
    }

    public class TestValidatableObject : IValidatableObject
    {
        public string Test { get; set; }

        public string Test2 { get; set; }

        public IEnumerable<DataAnnotationsValidationResult> Validate(ValidationContext validationContext)
        {
            yield return new DataAnnotationsValidationResult("Error without member");
            yield return new DataAnnotationsValidationResult("Error with member", new[] { nameof(Test) });
            yield return new DataAnnotationsValidationResult("Error with multiple members", new[] { nameof(Test), nameof(Test2) });
            yield return null;
        }
    }

    public class RecursiveTestObject
    {
        [Required]
        public RecursiveTestObject Test { get; set; }
    }
}