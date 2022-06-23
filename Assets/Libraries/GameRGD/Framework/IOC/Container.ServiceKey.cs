using System;

namespace Helen
{
    public struct ServiceKey
    {
        private readonly object value;

        public ServiceKey(object value)
        {
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is ServiceKey)
                return Equals((ServiceKey)obj);
            return false;
        }

        private bool Equals(ServiceKey obj)
        {
            if (obj.value == null)
                return value == null;
            return obj.value.Equals(value);
        }

        public override int GetHashCode()
        {
            return value == null ? 0 : value.GetHashCode();
        }

        public override string ToString()
        {
            if (value == null)
                return "null";
            return value.ToString();
        }

        public static implicit operator ServiceKey(Enum value)
        {
            return new ServiceKey(value);
        }

        public static implicit operator ServiceKey(string value)
        {
            return new ServiceKey(value);
        }

        public static implicit operator ServiceKey(int value)
        {
            return new ServiceKey(value);
        }

        public static implicit operator ServiceKey(long value)
        {
            return new ServiceKey(value);
        }

        public static implicit operator ServiceKey(float value)
        {
            return new ServiceKey(value);
        }

        public static implicit operator ServiceKey(double value)
        {
            return new ServiceKey(value);
        }

        public static implicit operator ServiceKey(decimal value)
        {
            return new ServiceKey(value);
        }

        public static implicit operator ServiceKey(bool value)
        {
            return new ServiceKey(value);
        }

        public static bool operator ==(ServiceKey left, ServiceKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ServiceKey left, ServiceKey right)
        {
            return !(left == right);
        }
    }
}