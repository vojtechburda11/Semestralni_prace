using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DataEntity;
public abstract class BaseModel : IDataErrorInfo
{
    [Timestamp] public byte[] RowVersion { get; set; } = [];
    public DateTime DatumVytvoreni { get; set; } = DateTime.UtcNow;
    [NotMapped] public int ErrorsCount => ValidateAll().Count;
    [NotMapped] public bool IsValid => ErrorsCount == 0;
    string IDataErrorInfo.Error => string.Join("\n", ValidateAll().Select(x => x.ErrorMessage));
    string IDataErrorInfo.this[string name] {
        get {
            var p = GetType().GetProperty(name);
            if (p == null) return "";
            var results = new List<ValidationResult>();
            Validator.TryValidateProperty(p.GetValue(this), new ValidationContext(this) { MemberName = name }, results);
            return string.Join("\n", results.Select(x => x.ErrorMessage));
        }
    }
    public IReadOnlyList<ValidationResult> ValidateAll() {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(this, new ValidationContext(this), results, true);
        return results;
    }
}
