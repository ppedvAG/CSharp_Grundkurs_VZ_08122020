using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Modul016_API_Console
{
    class Program
    {
        static HttpClient client = new HttpClient();
        static string apiUrl = "api/todoitems/";
        static void Main(string[] args)
        {
            //HTTP-Client konfigurieren (nur JSON akzeptieren)
            client.BaseAddress = new Uri("https://www.meine-domain.de/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                TodoItem todoitem = new TodoItem()
                {
                    Name = "Mein neues Item",
                    IsComplete = false
                };
                //TodoItem erstellen
                Uri url = Post(todoitem);
                Console.WriteLine($"Erstellt mit Url {url}");

                //TodoItem abfragen
                todoitem = Get(url.PathAndQuery);
                TodoItemInfo(todoitem);

                //TodoItem aendern
                todoitem.Name = "Ich habe einen neuen Namen";
                Put(todoitem);

                //geaendertes TodoItem abfragen
                todoitem = Get(url.PathAndQuery);
                TodoItemInfo(todoitem);

                //TodoItem loeschen
                HttpStatusCode code = Delete(todoitem.Id);
                Console.WriteLine($"Item wurde geloescht: Statuscode {(int)code}");

                //TodoItems abfragen
                List<TodoItem> items = Get();
                foreach (TodoItem item in items)
                {
                    TodoItemInfo(item);
                }
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }

        static void TodoItemInfo(TodoItem item)
        {
            Console.WriteLine($"Id: {item?.Id}\nName:{item?.Name}\nIdComplete:{item?.IsComplete}");
        }

        //JSON aus TodoItem erstellen
        static string CreateJson(TodoItem item)
        {
            return JsonSerializer.Serialize(item, typeof(TodoItem));
        }


        //TodoItem aus JSON erstellen
        static TodoItem CreateTodoItem(string json)
        {
            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<TodoItem>(json, options);
            }
            catch
            {
                return new TodoItem();
            }
        }

        //TodoItems aus JSON erstellen
        static List<TodoItem> CreateTodoItems(string json)
        {
            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<List<TodoItem>>(json, options);
            }
            catch
            {
                return null;
            }
        }

        //POST eines TodoItems
        static Uri Post(TodoItem item)
        {
            try
            {
                //Item als JSON in einer HttpContent abbilden
                HttpContent content = new StringContent(CreateJson(item), Encoding.UTF8, "application/json");

                //POST mit dem HttpContent an den WebClient senden
                HttpResponseMessage response = client.PostAsync(apiUrl, content).Result;

                //wirft einen Fehler wenn die Response nicht erfolgreich war
                response.EnsureSuccessStatusCode();
                
                //gibt die URL zurueck (api/todoitems/id)
                return response.Headers.Location;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        //GET
        static List<TodoItem> Get()
        {
            HttpResponseMessage response = client.GetAsync(apiUrl).Result;
            if (response.IsSuccessStatusCode)
            {
                return CreateTodoItems(response.Content.ReadAsStringAsync().Result);
            }

            return new List<TodoItem>();
        }

        //GET mit der Id und dem kompletten Pfad (apiUrl/Id)
        static TodoItem Get(string path)
        {
            TodoItem item = null;
            HttpResponseMessage response = client.GetAsync(path).Result;
            if (response.IsSuccessStatusCode)
            {
                item = CreateTodoItem(response.Content.ReadAsStringAsync().Result);
            }
            return item;
        }

        //GET mit der Id (Primaerschluessel)
        static TodoItem Get(long id)
        {
            return Get($"{apiUrl}{id}");
        }

        //PUT eines TodoItems
        static TodoItem Put(TodoItem item)
        {
            HttpContent content = new StringContent(CreateJson(item), Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PutAsync($"{apiUrl}{ item.Id}", content).Result;

            response.EnsureSuccessStatusCode();

            return CreateTodoItem(response.Content.ReadAsStringAsync().Result);
        }

        //DELETE mit der Id (Primaerschluessel)
        static HttpStatusCode Delete(long id)
        {
            HttpResponseMessage response = client.DeleteAsync($"{apiUrl}{id}").Result;
            return response.StatusCode;
        }
    }
}
