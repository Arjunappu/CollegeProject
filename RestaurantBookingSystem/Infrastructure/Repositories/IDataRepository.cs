using System.Collections.Generic;

namespace RestaurantBookingSystem.Infrastructure.Repositories
{
    /// <summary>
    /// An Interface for a typical Rpository with methods for basic CRUD Operations
    /// </summary>
    /// <typeparam name="T">The Type of the Repository</typeparam>
    public interface IDataRepository<T>
    {
        /// <summary>
        /// Finds a Record with the specified Id
        /// </summary>
        /// <param name="id">The id of the record</param>
        /// <returns>Resturns the Record if operation is successful otherwise returns null</returns>
        T Find(int id);

        /// <summary>
        /// Selects all the reords from the Database and Returns them
        /// </summary>
        /// <returns>Returns the IEnumerable List of found records</returns>
        IEnumerable<T> SelectAll();

        /// <summary>
        /// Adds a new Record to DataBase
        /// </summary>
        /// <param name="record">Record to add</param>
        /// <returns>Returns the Id of added Item if operation is successful otherwise returns -1</returns>
        int Add(T record);

        /// <summary>
        /// Updates the Available Record with new Details
        /// </summary>
        /// <param name="record">The Modified Record which needs to be updated</param>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        bool Update(T record);

        /// <summary>
        /// Deletes the record with specified id
        /// </summary>
        /// <param name="id">The id of the record that needs to be deleted</param>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        bool Delete(int id);
    }
}