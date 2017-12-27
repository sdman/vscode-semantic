﻿namespace CompillerServices.Frontend.NameTables
{
    public class ArgumentNameTable : NameTable<ArgumentNameTableRow>
    {
    }

    public class ArgumentNameTableRow : NameTableRow
    {
        public ArgumentNameTableRow(int line, int column, string passModifier, string type, string name,
            SubprogramNameTableRow parent)
            : base(line, column)
        {
            PassModifier = passModifier;
            Type = type;
            Name = name;
            Parent = parent;
        }

        public string PassModifier { get; }
        public string Type { get; }
        public string Name { get; }

        public SubprogramNameTableRow Parent { get; }
    }
}