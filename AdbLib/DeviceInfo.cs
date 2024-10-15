namespace AdbLib;

public sealed class DeviceInfo(string name, string serial)
{
  public string Name { get; private set; } = name;
  public string Serial { get; private set; } = serial;

  public static bool operator ==(DeviceInfo left, DeviceInfo right)
  {
    if (ReferenceEquals(left, right)) return true;
    if (left is null || right is null) return false;
    return left.Equals(right);
  }

  public static bool operator !=(DeviceInfo left, DeviceInfo right)
  {
    return !(left == right);
  }

  public override bool Equals(object? obj)
  {
    if (obj is DeviceInfo other)
    {
      return Name == other.Name && Serial == other.Serial;
    }
    return false;
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(Name, Serial);
  }
}
