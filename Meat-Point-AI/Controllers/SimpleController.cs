using Meat_Point_AI.App_Data;
using Meat_Point_AI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Meat_Point_AI.Controllers
{
    
    // Beef Meal Planning Controllers
    public class UserController : SimpleController<Users> { }
    public class RecipeController : SimpleController<Recipes> { }
    public class BeefCutController : SimpleController<BeefCuts> { }
    public class MealPlanController : SimpleController<MealPlans> { }
    public class RecipeRatingController : SimpleController<RecipeRatings> { }

    public abstract class SimpleController<T> : ApiController
    {
        private string table;
        public SimpleController()
        {
            table = typeof(T).Name;
        }

        // GET api/<controller>
        public IEnumerable<T> Get()
        {
            return DAL.select<T>("select * from " + table);
        }

        // GET api/<controller>/5
        public T Get(int id)
        {
            //remore the s' (of plural) from table name for the id column
            string singularTableName = table.EndsWith("s") ? table.Substring(0, table.Length-1) : table;
            return DAL.select<T>($"select * from {table} where {singularTableName}ID = {id}").FirstOrDefault();
        }

        // POST api/<controller>
        public int Post([FromBody] T item)
        {
            return DAL.insert(item); 
        }

        // PUT api/<controller>/5
        public void Put([FromBody] T item)
        {
            DAL.update(item);
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
            DAL.Delete(table, id);
        }
    }
}