using System;

class Program {
    static void Main() {
        object dbValue = 123;
        try {
            string s = (string)dbValue;
        } catch(Exception ex) {
            Console.WriteLine("Casting Int32 (DB) to string (C#): " + ex.Message);
        }

        object dbValueStr = "123";
        try {
            int i = (int)dbValueStr;
        } catch(Exception ex) {
            Console.WriteLine("Casting string (DB) to Int32 (C#): " + ex.Message);
        }
    }
}
