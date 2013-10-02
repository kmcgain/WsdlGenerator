namespace CSGeneration
{
    public class ArgumentDescription
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name)
                       ? Value
                       : Name + "=" + Value;
        }
    }
}