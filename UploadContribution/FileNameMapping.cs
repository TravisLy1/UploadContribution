using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UploadContribution
{
    interface IFileNameMapping
    {
        string CreateDestination(string sourceName);
    }
    /// <summary>
    /// This class maps the source file name and create a corresponding Destination Path
    /// </summary>
    class FileNameMapping: IFileNameMapping
    {
        private List<ProductTag> _tagList;

        public FileNameMapping()
        {
            _tagList = new List<ProductTag>();
            _tagList.Add(new ProductTag("BackupReporter", "Backup_Reporter"));
            _tagList.Add(new ProductTag("BenchmarkFactory", "Benchmark_Factory"));
            _tagList.Add(new ProductTag("Spotlight on Oracle", "Performance_Diagnostics_(Spotlight_On_Oracle)", "x64"));
            _tagList.Add(new ProductTag("QuestSQLOptimizerForOracle", "QuestSQLOptimizer_Oracle"));
            _tagList.Add(new ProductTag("ToadDataModeler", "Toad_Data_Modeler"));
            _tagList.Add(new ProductTag("DellCodeTester", "Quest_Code_Tester")); 
            _tagList.Add(new ProductTag("ToadforSQLServer", "Toad_For_MSSQL"));
            _tagList.Add(new ProductTag("ToadforMySQL_Freeware", "Toad_For_MySQL_Freeware"));
            _tagList.Add(new ProductTagToad("Dell Toad for Oracle", "Toad"));
        }
               
        // input =  BenchmarkFactory_7_1_0_496_32bit.msi
        // output is a folder Benchnark_Factory 
        public string CreateDestination(string sourceName)
        {
            string destName = sourceName;
            string productTagfromName = ProductTag.GetMainTag(sourceName);      // get the product tag, ignore numbers 

            // loop thru the list to see if any match
            foreach (ProductTag p in _tagList)
            {
                if (productTagfromName.StartsWith(p.Key))
                {
                    // found the main part matches
                    destName = p.GetResultValue(sourceName) + "/" + sourceName;
                    break;
                }
            }
            return destName;
        }
    }
}
