namespace JsonSchemaToCsClass
{
    internal static class UniqueIndex
    {
        public static int GetNext()
        {
            return _currentId++;
        }

        private static int _currentId;
    }
}
