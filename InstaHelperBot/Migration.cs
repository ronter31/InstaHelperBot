using Npgsql;

namespace InstaBotHelper
{
    public class Migration
    {
        public Migration(string connString)
        {
            using (var connection = new NpgsqlConnection(connString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    DO
                    $$
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM information_schema.tables WHERE table_name = 'DictionaryReplace') THEN
                            CREATE TABLE IF NOT EXISTS public.""DictionaryReplace""
(
    ""Id"" integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    ""Regular"" character varying(1000) COLLATE pg_catalog.""default"",
    ""Replace"" character varying(1000) COLLATE pg_catalog.""default"",
    CONSTRAINT ""DictionaryReplace_pkey"" PRIMARY KEY (""Id"")
);
                        END IF;
                    END
                    $$";
                command.ExecuteNonQuery();

                command.CommandText = @"
                    DO
                    $$
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM information_schema.tables WHERE table_name = 'Posts') THEN
                            CREATE TABLE IF NOT EXISTS public.""Posts""
(
    ""Id"" integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    ""IdPosts"" bigint,
    ""Status"" character varying(100) COLLATE pg_catalog.""default"",
    ""Type"" character varying(100) COLLATE pg_catalog.""default"",
    CONSTRAINT ""Posts_pkey"" PRIMARY KEY (""Id"")
);
                        END IF;
                    END
                    $$";
                command.ExecuteNonQuery();


                command.CommandText = @"
                    DO
                    $$
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM information_schema.tables WHERE table_name = 'TelegramGroup') THEN
                            CREATE TABLE IF NOT EXISTS public.""TelegramGroup""
(
    id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    ""NameCodeGroup"" character varying(500) COLLATE pg_catalog.""default"",
    CONSTRAINT ""TelegramGroup_pkey"" PRIMARY KEY (id)
);
                        END IF;
                    END
                    $$";
                command.ExecuteNonQuery();


                command.CommandText = @"
                    DO
                    $$
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM information_schema.tables WHERE table_name = 'User') THEN
                            CREATE TABLE IF NOT EXISTS public.""User""
(
    ""Id"" integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    ""IdUniq"" character varying(500) COLLATE pg_catalog.""default"",
    ""Role"" character varying(100) COLLATE pg_catalog.""default"",
    ""Token"" character varying(600) COLLATE pg_catalog.""default"",
    CONSTRAINT ""User_pkey"" PRIMARY KEY (""Id"")
);
                        END IF;
                    END
                    $$";
                command.ExecuteNonQuery();


                command.CommandText =  @"
                    DO
                    $$
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM information_schema.tables WHERE table_name = 'Аccount') THEN
                            CREATE TABLE IF NOT EXISTS public.""Аccount""
(
    ""Id"" integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    ""TypeAcc"" character varying(250) COLLATE pg_catalog.""default"",
    ""UserName"" character varying(350) COLLATE pg_catalog.""default"",
    ""Password"" character varying(350) COLLATE pg_catalog.""default"",
    CONSTRAINT ""Аccount_pkey"" PRIMARY KEY (""Id"")
);
                        END IF;
                    END
                    $$";
                command.ExecuteNonQuery();

                connection.Close();
            }
        }

    }
}
