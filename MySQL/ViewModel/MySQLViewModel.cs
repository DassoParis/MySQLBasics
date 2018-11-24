using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace MySQL
{
    /// <summary>
    /// The View Model for the custom flat window
    /// </summary>
    public class MySQLViewModel : BaseViewModel
    {
        #region Private Properties

        private object ClassLock = new object();

        private enum RegexType { Alpha, Numeric };

        private Dictionary<RegexType, string> RegexDict = new Dictionary<RegexType, string>  { { RegexType.Alpha, @"^[a-zA-Z0-9áàâäãåçéèêëíìîïñóòôöõúùûüýÿÁÀÂÄÃÅÇÉÈÊËÍÌÎÏÑÓÒÔÖÕÚÙÛÜÝŸ]{1,60}$" },
                                                                                               { RegexType.Numeric, @"^[0-9]+$"} };

        private enum ReadingType { Inserting, Searching, Updating, Deleting, First, Last, Next, Previous };

        private Dictionary<ReadingType, int> FocusDict = new Dictionary<ReadingType, int>  { { ReadingType.Searching, -1 }, { ReadingType.Updating, -1 },
                                                                                             { ReadingType.Deleting, -1 },  { ReadingType.First, -1 },
                                                                                             { ReadingType.Last, -1 },      { ReadingType.Next, -1 },
                                                                                             { ReadingType.Previous, -1 } };        

        #endregion

        #region Public Properties

        /// <summary>
        /// The classe that represents a collection of student
        /// </summary>
        public ObservableCollection<Student> Class { get; set; }  = new ObservableCollection<Student>();

        /// <summary>
        /// The classe selected index
        /// </summary>
        public int ClassSelectedIndex { get; set; }

        /// <summary>
        /// The classe selected item
        /// </summary>
        public object ClassSelectedItem { get; set; }

        /// <summary>
        /// The ID of the student
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The firstname of the student
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The lastname of the student
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The age of the student
        /// </summary>
        public string Age { get; set; }

        #endregion

        #region Commands

        /// <summary>
        /// The command to reset the form
        /// </summary>
        public ICommand ResetCommand { get; set; }

        /// <summary>
        /// The command to insert data in the database
        /// </summary>
        public ICommand InsertCommand { get; set; }

        /// <summary>
        /// The command to search data in the database
        /// </summary>
        public ICommand SearchCommand { get; set; }

        /// <summary>
        /// The command to update data in the database
        /// </summary>
        public ICommand UpdateCommand { get; set; }

        /// <summary>
        /// The command to delete data in the database
        /// </summary>
        public ICommand DeleteCommand { get; set; }

        /// <summary>
        /// The command that move to the first row in the database
        /// </summary>
        public ICommand FirstCommand { get; set; }

        /// <summary>
        /// The command that move to the last row in the database
        /// </summary>
        public ICommand LastCommand { get; set; }

        /// <summary>
        /// The command that move to the next row in the database
        /// </summary>
        public ICommand NextCommand { get; set; }

        /// <summary>
        /// The command that move to the previous row in the database
        /// </summary>
        public ICommand PreviousCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public MySQLViewModel(Window window)
        {
            /* window events */

            window.Loaded += (sender, e) =>
            {              
            };

            window.Closed += (sender, e) =>
            {                
            };

            /* Cross-Thread collection synchronization  */

            BindingOperations.CollectionRegistering += (sender, e) =>
            {
                if (e.Collection == Class)
                {
                    BindingOperations.EnableCollectionSynchronization (Class, ClassLock);
                }
            };

            /* Update the HMI => student array */

            ReadDataAsync (ReadingType.Inserting);

            /* Create commands */

            ResetCommand = new RelayCommand (() => ResetStudentForm());

            InsertCommand = new RelayCommand (async () => await InsertAsync());
            SearchCommand = new RelayCommand (async () => await SearchDataAsync());
            UpdateCommand = new RelayCommand (async () => await UpdateDataAsync());
            DeleteCommand = new RelayCommand (async () => await DeleteDataAsync());

            FirstCommand = new RelayCommand(async () => await FirstAsync());
            LastCommand = new RelayCommand(async () => await LastDataAsync());
            NextCommand = new RelayCommand(async () => await NextDataAsync());
            PreviousCommand = new RelayCommand(async () => await PreviousDataAsync());
        }

        #endregion

        #region Items management

        private void GetFocus (ReadingType dataBaseAction)
        {
            if (Class.Count == 0)
                return;

            /* Set the selected index */

            switch (dataBaseAction)
            {
                case ReadingType.Inserting:

                    ClassSelectedIndex = Class.Count - 1;
                    break;

                default:
                    
                    ClassSelectedIndex = FindIndexfromId (FocusDict[dataBaseAction]);
                    break;
            }
        }

        private int IndexNormalisation (int index)
        {
            if (index >= Class.Count)
            {
                return 0;
            }
            else if (index < 0)
            {
                return Class.Count - 1;
            }
            else
            {
                return index;
            }
        }

        private int FindIndexfromId (int id)
        {
            for (int i = 0; i < Class.Count; i++)
            {
                if ((Class[i]?.ID ?? -1) == id)
                {
                    return i;
                }
            }

            return -1;
        }

        private int FindIdfromIndex (int index)
        {
            if (index >= Class.Count)
                return -1;

            return Class[index]?.ID ?? -1;
        }

        private void ResetStudentForm ()
        {
            // ID
            ID = "";

            // First name
            FirstName = "";

            // Last name
            LastName = "";

            // Age
            Age = "";
        }

        private Status SetStudentForm (string id, string firstName, string lastName, string age)
        {
            if (string.IsNullOrWhiteSpace(firstName) == true || string.IsNullOrWhiteSpace(firstName) == true ||
                string.IsNullOrWhiteSpace(lastName) == true || string.IsNullOrWhiteSpace(age) == true)
            {
                return Status.Failed;
            }
            else
            {
                // ID
                ID = id;

                // First name
                FirstName = firstName;

                // Last name
                LastName = lastName;

                // Age
                Age = age;

                return Status.Successful;
            }
        }

        private (Status, Student) GetStudentFromId (int id)
        {
            Status status = Status.Failed;

            Student student = new Student();

            // Check if the number of classe is empty
            if (Class.Count == 0) goto End;

            // Search for the student information
            for (int i = 0; i < Class.Count; i++)
            {
                if ((Class[i]?.ID ?? -1) == id)
                {
                    // Get the student information
                    student.ID = Class[i]?.ID ?? -1;
                    student.FirstName = Class[i]?.FirstName ?? string.Empty;
                    student.LastName = Class[i]?.LastName ?? string.Empty;
                    student.Age = Class[i]?.Age ?? -1;
                }
            }

            // Check if the value are valid
            if (student.ID == -1 || string.IsNullOrWhiteSpace(student.FirstName) == true ||
                string.IsNullOrWhiteSpace(student.LastName) == true || student.Age == -1)
            {
                status = Status.Failed;
            }
            else
            {
                status = Status.Successful;
            }

            // End
            //********************************************************************************

            End:

            return (status, student);
        }

        private (Status, Student) GetStudentFromIndex (int index)
        {
            Status status = Status.Failed;

            Student student = new Student();

            // Check if the index is valid
            if (index < 0 && index >= Class.Count) goto End;

            // Get the student information
            student.ID = Class[index]?.ID ?? -1;
            student.FirstName = Class[index]?.FirstName ?? string.Empty;
            student.LastName = Class[index]?.LastName ?? string.Empty;
            student.Age = Class[index]?.Age ?? -1;

            // Check if the value are valid
            if (student.ID == -1 || string.IsNullOrWhiteSpace(student.FirstName) == true ||
                string.IsNullOrWhiteSpace(student.LastName) == true || student.Age == -1)
            {
                status = Status.Failed;
            }
            else
            {
                status = Status.Successful;
            }

            // End
            //********************************************************************************

            End:

            return (status, student);
        }

        #endregion

        #region Read data

        /// <summary>
        /// Read data from the database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ReadDataAsync (ReadingType dataBaseAction)
        {            
            // Update student array 
            var (status, tempClasse) = await Task.Run(() => ReadData());

            if (status == Status.Successful)
            {
                // Clear the list of student
                Class.Clear();

                // Update the list of student
                tempClasse.ForEach (val => Class.Add(val));
            }

            // Get focus to the appropriete item
            GetFocus (dataBaseAction);
        }

        private (Status, List<Student>) ReadData()
        {
            // Format the select request
            string query = "SELECT * FROM test.classe";

            // Database read request 
            var (status, tempClasse) = ReadRequest (query, "Error: database reading", Database.ReadingType.DataReader);

            return (status, tempClasse);
        }

        private (Status, List<Student>) ReadRequest(string query, string errorMessageTitle, Database.ReadingType type)
        {
            Status status = Status.Failed;

            List<Student> tempClasse = new List<Student>();

            if (type == Database.ReadingType.DataAdapter)
            {
                (status, tempClasse) = DataAdapterRequest(query, errorMessageTitle);
            }
            else if (type == Database.ReadingType.DataReader)
            {
                (status, tempClasse) = DataReaderRequest(query, errorMessageTitle);
            }

            return (status, tempClasse);
        }

        private (Status, List<Student>) DataAdapterRequest(string query, string errorMessageTitle)
        {
            Status status = Status.Failed;

            List<Student> tempClasse = new List<Student>();

            DataTable table = new DataTable();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(Database.ConnectionString))
                {
                    // Connection opening
                    connection.Open();

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection))
                    {
                        // Fill database table
                        adapter.Fill(table);

                        // Convert database table to list of student 
                        foreach (DataRow row in table.Rows)
                        {
                            // Update status
                            status = Status.Successful;

                            // Fill the list of student
                            tempClasse.Add(new Student()
                            {
                                ID = row.ItemArray[0].ParseInt(),
                                FirstName = row.ItemArray[1].ParseString(),
                                LastName = row.ItemArray[2].ParseString(),
                                Age = row.ItemArray[3].ParseInt()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, errorMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return (status, tempClasse);
        }

        private (Status, List<Student>) DataReaderRequest(string query, string errorMessageTitle)
        {
            Status status = Status.Failed;

            List<Student> tempClasse = new List<Student>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(Database.ConnectionString))
                {
                    // Connection opening
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Execute the command 
                        using (MySqlDataReader mysqlReader = command.ExecuteReader())
                        {
                            // Check if there is any row left from the executed command  
                            while (mysqlReader.Read() == true)
                            {                              
                                // Fill the list of student
                                tempClasse.Add(new Student()
                                {
                                    ID = mysqlReader.GetInt16(0),
                                    FirstName = mysqlReader.GetString(1),
                                    LastName = mysqlReader.GetString(2),
                                    Age = mysqlReader.GetInt16(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, errorMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Update status
            status = (tempClasse.Count > 0) ? Status.Successful : Status.Failed;

            return (status, tempClasse);
        }

        #endregion

        #region Insert data 

        /// <summary>
        /// Insert data in the database
        /// </summary>
        /// <returns></returns>
        private async Task InsertAsync ()
        {
            var status = await Task.Run(() => InsertData());

            if (status == Status.Successful)
            {
                // Update the HMI => student array
                ReadDataAsync (ReadingType.Inserting);
            } 
        }

        private Status InsertData()
        {
            Status status = Status.Failed;
            
            // Trim inputs before and after, help to reduce memory space in the database
            string firstName = FirstName?.Trim(), lastName = LastName?.Trim(), age = Age?.Trim();

            // Check if inputs are empty
            if (string.IsNullOrWhiteSpace(firstName) == true || string.IsNullOrWhiteSpace(lastName) == true || string.IsNullOrWhiteSpace(age) == true)
                goto End;

            // Check if inputs are valid data
            if (Regex.IsMatch (firstName, RegexDict[RegexType.Alpha]) == false || 
                Regex.IsMatch (lastName, RegexDict[RegexType.Alpha]) == false  || 
                Regex.IsMatch (age, RegexDict[RegexType.Numeric]) == false)

                goto End;

            // Format the insert request
            string query = "INSERT INTO test.classe(firstName, lastName, age) VALUES('" + firstName + "', '" + lastName + "', " + age + ")";
            
            // Database insert request
            status = InsertRequest (query, "Error: database inserting");

            // End
            //********************************************************************************

            End:

            return status;
        }

        private Status InsertRequest (string query, string errorMessageTitle)
        {
            int affectedRows = 0;
            
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Database.ConnectionString))
                {
                    // Connection opening
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Execute the command 
                        affectedRows = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, errorMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return (affectedRows > 0) ? Status.Successful : Status.Failed;
        }

        #endregion

        #region Search data

        private async Task SearchDataAsync ()
        {
            // Reset student form
            ResetStudentForm();

            var (status, student) = await Task.Run(() => SearchData());

            if (status == Status.Successful)
            {
                // Set the student form
                status = SetStudentForm (student.ID.ToString(),
                                         student.FirstName,
                                         student.LastName,
                                         student.Age.ToString());

                // Update current ID
                FocusDict[ReadingType.Searching] = student.ID;

                // Get focus to the appropriete item
                GetFocus (ReadingType.Searching);
            }            
        }

        private (Status, Student) SearchData ()
        {
            Status status = Status.Failed;

            Student student = new Student();

            // Trim ID before and after 
            string id = ID?.Trim();                     

            // Check if ID is empty
            if (string.IsNullOrWhiteSpace(id) == true)
            {
                // Current selected index
                var index = ClassSelectedIndex;

                // ID to delete
                id = FindIdfromIndex(index).ToString();
            }

            // Check if the ID is empty
            if (string.IsNullOrWhiteSpace(id) == true) goto End;

            // Check if the ID is valid data
            if (Regex.IsMatch(id, RegexDict[RegexType.Numeric]) == false) goto End;

            // Format the search request 
            string query = "SELECT* FROM test.classe WHERE id=" + id;

            // Search request in or out database 
            (status, student) = SearchRequest (query, "Error: database searching", id, Database.SearchingType.NoDatabase);

            // End
            //********************************************************************************

            End:

            return (status, student);
        }

        private (Status, Student) SearchRequest (string query, string errorMessageTitle, string id, Database.SearchingType type)
        {
            Status status = Status.Failed;

            Student student = new Student();

            if (type == Database.SearchingType.WithDatabase)
            {               
                (status, student) = GetStudentFromDatabase (query, errorMessageTitle);
            }
            else if (type == Database.SearchingType.NoDatabase)
            {
                (status, student) = GetStudentFromId (id.ParseInt());
            }

            return (status, student);
        }

        private (Status, Student) GetStudentFromDatabase (string query, string errorMessageTitle)
        {
            Status status = Status.Failed;

            Student student = new Student();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(Database.ConnectionString))
                {
                    // Connection opening
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Execute the command 
                        using (MySqlDataReader mysqlReader = command.ExecuteReader())
                        {
                            // Check if there is any row left from the executed command  
                            if (mysqlReader.Read() == true)
                            {                                
                                // Get the student information
                                student.ID = mysqlReader.GetInt16 (0);
                                student.FirstName = mysqlReader.GetString (1);
                                student.LastName = mysqlReader.GetString (2);
                                student.Age = mysqlReader.GetInt16 (3);

                                // Update status
                                status = Status.Successful;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, errorMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return (status, student);
        }
        
        #endregion

        #region Update data

        private async Task UpdateDataAsync ()
        {                    
            var (status, id) = await Task.Run(() => UpdateData());

            if (status == Status.Successful)
            {
                // Update current ID
                FocusDict[ReadingType.Updating] = id.ParseInt();

                // Update the HMI => student array
                ReadDataAsync (ReadingType.Updating);
            }            
        }

        private (Status, string) UpdateData ()
        {
            Status status = Status.Failed;

            // Trim inputs before and after 
            string id = ID?.Trim(), firstName = FirstName?.Trim(), lastName = LastName?.Trim(), age = Age?.Trim();

            // Check if ID is empty
            if (string.IsNullOrWhiteSpace(id) == true)
            {
                // Current selected index
                var index = ClassSelectedIndex;

                // ID to delete
                id = FindIdfromIndex(index).ToString();
            }

            // Check if inputs are empty
            if (string.IsNullOrWhiteSpace(id) == true || string.IsNullOrWhiteSpace(firstName) == true ||
                string.IsNullOrWhiteSpace(lastName) == true || string.IsNullOrWhiteSpace(age) == true)

                goto End;

            // Check if inputs are valid data
            if (Regex.IsMatch(id, RegexDict[RegexType.Numeric]) == false || Regex.IsMatch(firstName, RegexDict[RegexType.Alpha]) == false || 
                Regex.IsMatch(lastName, RegexDict[RegexType.Alpha]) == false || Regex.IsMatch(age, RegexDict[RegexType.Numeric]) == false)

                goto End;

            // Format the update request
            string query = "UPDATE test.classe SET firstName='" + firstName + "', lastName='" + lastName + "', age =" + age + " WHERE id=" + id;

            // Database update request 
            status = UpdateRequest (query, "Error: database updating");

            // End
            //********************************************************************************

            End:

            return (status, id);
        }

        private Status UpdateRequest (string query, string errorMessageTitle)
        {
            int affectedRows = 0;

            try
            {
                using (MySqlConnection connection = new MySqlConnection(Database.ConnectionString))
                {
                    // Connection opening
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Execute the command 
                        affectedRows = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, errorMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return (affectedRows > 0) ? Status.Successful : Status.Failed;
        }
        
        #endregion

        #region Delete data

        private async Task DeleteDataAsync ()
        {
            var (status, student) = await Task.Run(() => DeleteData ());

            if (status == Status.Successful)
            {
                // Set the student form
                status = SetStudentForm (student.ID.ToString(),
                                         student.FirstName,
                                         student.LastName,
                                         student.Age.ToString());

                // Get the index of the current student 
                var index = FindIndexfromId (student.ID);

                // Get the ID of the next student 
                var id = FindIdfromIndex (index + 1);

                // Current selected index
                FocusDict[ReadingType.Deleting] = id;

                // Update the HMI => student array
                ReadDataAsync (ReadingType.Deleting);
            } 
        }

        private (Status, Student) DeleteData ()
        {            
            Status status = Status.Failed;

            Student student = new Student();

            // Get current selected index 
            int index = ClassSelectedIndex;

            // Index to delete
            int id = FindIdfromIndex (ClassSelectedIndex);
            if (id < 0) goto End;            

            // Format the delete request
            string query = "DELETE FROM test.classe WHERE id=" + id;

            // Database delete request 
            status = DeleteRequest (query, "Error: database deleting");
            if (status == Status.Failed) goto End;

            // Get the deleted student 
            (status, student) = GetStudentFromIndex (index);

            // End
            //********************************************************************************

            End:

            return (status, student);
        }

        private Status DeleteRequest(string query, string errorMessageTitle)
        {
            int affectedRows = 0;

            try
            {
                using (MySqlConnection connection = new MySqlConnection(Database.ConnectionString))
                {
                    // Connection opening
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Execute the command 
                        affectedRows = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show (ex.Message, errorMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return (affectedRows > 0) ? Status.Successful : Status.Failed;
        }

        #endregion

        #region First

        private async Task FirstAsync ()
        {
            // Reset student form
            ResetStudentForm();

            var (status, student) = await Task.Run(() => FirstData ());

            if (status == Status.Successful)
            {
                // Set the student form
                status = SetStudentForm (student.ID.ToString(),
                                         student.FirstName,
                                         student.LastName,
                                         student.Age.ToString());

                // Update the current ID
                FocusDict[ReadingType.First] = student.ID;

                // Get focus to the appropriete item
                GetFocus (ReadingType.First);
            }
        }

        private (Status, Student) FirstData ()
        {          
            // Get the first student 
            var (status, student) = GetStudentFromIndex (0);            

            return (status, student);
        }

        #endregion

        #region Last

        private async Task LastDataAsync ()
        {
            // Reset student form
            ResetStudentForm();

            var (status, student) = await Task.Run(() => LastData());

            if (status == Status.Successful)
            {
                // Set the student form
                status = SetStudentForm (student.ID.ToString(),
                                         student.FirstName,
                                         student.LastName,
                                         student.Age.ToString());

                // Update the current ID
                FocusDict[ReadingType.Last] = student.ID;

                // Get focus to the appropriete item
                GetFocus(ReadingType.Last);
            }
        }

        private (Status, Student) LastData()
        {
            // Get the last student 
            var (status, student) = GetStudentFromIndex (Class.Count - 1);

            return (status, student);
        }

        #endregion

        #region Next

        private async Task NextDataAsync()
        {
            // Reset student form
            ResetStudentForm ();

            var (status, student) = await Task.Run(() => NextData());

            if (status == Status.Successful)
            {
                // Set the student form
                status = SetStudentForm (student.ID.ToString(),
                                         student.FirstName,
                                         student.LastName,
                                         student.Age.ToString());

                // Update the current ID
                FocusDict[ReadingType.Next] = student.ID;

                // Get focus to the appropriete item
                GetFocus(ReadingType.Next);
            }            
        }

        private (Status, Student) NextData()
        {
            Status status = Status.Failed;

            Student student = new Student();

            // Trim ID before and after 
            string id = ID?.Trim();

            // Find the corresponding index 
            int index = FindIndexfromId (id.ParseInt());
                 
            // Check if the index is valid
            if (index == -1)
            {
                // Current selected index
                index = ClassSelectedIndex;
            }

            // Index normalisation
            var validIndex = IndexNormalisation (index + 1);

            // Get the next student 
            (status, student) = GetStudentFromIndex (validIndex);

            return (status, student);
        }

        #endregion

        #region Previous

        private async Task PreviousDataAsync ()
        {
            // Reset student form
            ResetStudentForm();

            var (status, student) = await Task.Run(() => PreviousData ());

            if (status == Status.Successful)
            {
                // Set the student form
                status = SetStudentForm (student.ID.ToString (),
                                         student.FirstName,
                                         student.LastName,
                                         student.Age.ToString ());

                // Update the current ID
                FocusDict[ReadingType.Previous] = student.ID;

                // Get focus to the appropriete item
                GetFocus(ReadingType.Previous);
            }
        }

        private (Status, Student) PreviousData ()
        {
            Status status = Status.Failed;

            Student student = new Student();

            // Trim ID before and after 
            string id = ID?.Trim();

            // Find the corresponding index 
            int index = FindIndexfromId (id.ParseInt());

            // Check if the index is valid
            if (index == -1)
            {
                // Current selected index
                index = ClassSelectedIndex;
            }

            // Index normalisation
            var validIndex = IndexNormalisation (index - 1);

            // Get the previous student 
            (status, student) = GetStudentFromIndex (validIndex);

            return (status, student);
        }

        #endregion
    }

    public class Student: BaseViewModel
    {
        public int ID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }
    }    
}
