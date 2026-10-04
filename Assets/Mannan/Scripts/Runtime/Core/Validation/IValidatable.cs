using System.Collections.Generic;

namespace Mannan.Core.Validation
{
    public interface IValidatable
    {
        void Validate(List<ValidationIssue> issues);
    }
}
