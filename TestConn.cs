using System;
using Npgsql;

class Program {
    static void Main() {
        string[] connectionStrings = new string[] {
            "Host=aws-0-us-east-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.wvhpapkblbrixzyskngg;Password=Cristi@n020820;Pooling=false;Timeout=10;",
            "Host=aws-0-us-east-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.wvhpapkblbrixzyskngg;Password=Cristi@n020820;Pooling=false;Timeout=10;",
            "Host=aws-0-us-east-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres;Password=Cristi@n020820;Pooling=false;Timeout=10;",
            "Host=aws-0-us-east-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres;Password=Cristi@n020820;Pooling=false;Timeout=10;"
        };

        foreach (var cs in connectionStrings) {
            Console.WriteLine($"Testing: {cs}");
            try {
                using var conn = new NpgsqlConnection(cs);
                conn.Open();
                Console.WriteLine("SUCCESS!");
                return;
            } catch (Exception ex) {
                Console.WriteLine($"FAILED: {ex.Message}");
            }
        }
    }
}
