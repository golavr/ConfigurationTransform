using System;
using EnvDTE;

namespace GolanAvraham.ConfigurationTransform.Services.Implementations
{
    //TODO:[Golan] - remove PropertyClass
    //public class PropertyClass : Property, IEquatable<PropertyClass>
    //{
    //    public object Value { get; set; }
    //    public string Name { private set; get; }

    //    public short NumIndices { get; private set; }
    //    public object Application { get; private set; }
    //    public Properties Parent { get; private set; }
    //    public Properties Collection { get; private set; }
    //    public object Object { get; set; }
    //    public DTE DTE { get; private set; }

    //    public PropertyClass(string name, object value)
    //    {
    //        Name = name;
    //        Value = value;
    //    }

    //    public void let_Value(object lppvReturn)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public object get_IndexedValue(object Index1, object Index2, object Index3, object Index4)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void set_IndexedValue(object Index1, object Index2, object Index3, object Index4, object Val)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    #region Equals

    //    public bool Equals(PropertyClass other)
    //    {
    //        if (ReferenceEquals(null, other)) return false;
    //        if (ReferenceEquals(this, other)) return true;
    //        return Equals(Value, other.Value) && string.Equals(Name, other.Name);
    //    }

    //    public override bool Equals(object obj)
    //    {
    //        if (ReferenceEquals(null, obj)) return false;
    //        if (ReferenceEquals(this, obj)) return true;
    //        if (obj.GetType() != this.GetType()) return false;
    //        return Equals((PropertyClass)obj);
    //    }

    //    public override int GetHashCode()
    //    {
    //        unchecked
    //        {
    //            return ((Value != null ? Value.GetHashCode() : 0) * 397) ^ (Name != null ? Name.GetHashCode() : 0);
    //        }
    //    }

    //    public static bool operator ==(PropertyClass left, PropertyClass right)
    //    {
    //        return Equals(left, right);
    //    }

    //    public static bool operator !=(PropertyClass left, PropertyClass right)
    //    {
    //        return !Equals(left, right);
    //    }

    //    #endregion // Equals
    //}
}