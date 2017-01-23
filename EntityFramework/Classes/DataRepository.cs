using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Objects;
using System.Data;
using System.Data.Metadata.Edm;
using System.Data.Objects.DataClasses;
using EntityFramework.Interfaces;

namespace EntityFramework.Classes
{
    /// <summary>
    /// A generic repository for working with data in the database
    /// </summary>
    /// <typeparam name="T">A POCO that represents an Entity Framework entity</typeparam>
    public class DataRepository<T, C> : IRepository<T, C>
        where T : EntityObject
        where C : ObjectContext
    {
        /// Contexto do Entity Framework
        private C _context;

        /// Contexto do Entity Framework
        public C Context
        {
            get { return _context; }
            set { _context = value; }
        }

        /// Retorna o nome do EntitySet do objeto persistente
        private string entitySetName;

        /// Retorna o nome do EntitySet do objeto persistente
        protected string EntitySetName
        {
            get
            {
                if (String.IsNullOrEmpty(entitySetName))
                {
                    entitySetName = GetEntitySetName(typeof(T).Name);
                }

                return entitySetName;
            }
        }

        ///// <summary>
        ///// The context object for the database
        ///// </summary>
        //private ObjectContext _context;

        /// <summary>
        /// The IObjectSet that represents the current entity.
        /// </summary>
        private IObjectSet<T> _objectSet;

        /// <summary>
        /// Initializes a new instance of the DataRepository class
        /// </summary>
        public DataRepository()
        {
            _context = ContextManager.GetContext<C>();
            _objectSet = _context.CreateObjectSet<T>();
        }

        /// <summary>
        /// Gets all records as an IQueryable
        /// </summary>
        /// <returns>An IQueryable object containing the results of the query</returns>
        public IQueryable<T> Fetch()
        {
            return _objectSet;
        }

        /// <summary>
        /// Gets all records as an IEnumberable
        /// </summary>
        /// <returns>An IEnumberable object containing the results of the query</returns>
        public IEnumerable<T> GetAll()
        {
            return Fetch().AsEnumerable();
        }

        /// <summary>
        /// Finds a record with the specified criteria
        /// </summary>
        /// <param name="predicate">Criteria to match on</param>
        /// <returns>A collection containing the results of the query</returns>
        public IEnumerable<T> Find(Func<T, bool> predicate)
        {
            return _objectSet.Where<T>(predicate);
        }
        
        public T EncontraItem(Func<T, bool> predicate)
        {
            if (this.Find(predicate).Count() > 0)
            {
                return this.First(predicate);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a single record by the specified criteria (usually the unique identifier)
        /// </summary>
        /// <param name="predicate">Criteria to match on</param>
        /// <returns>A single record that matches the specified criteria</returns>
        public T Single(Func<T, bool> predicate)
        {
            return _objectSet.Single<T>(predicate);
        }

        /// <summary>
        /// The first record matching the specified criteria
        /// </summary>
        /// <param name="predicate">Criteria to match on</param>
        /// <returns>A single record containing the first record matching the specified criteria</returns>
        public T First(Func<T, bool> predicate)
        {
            return _objectSet.First<T>(predicate);
        }

        /// <summary>
        /// Deletes the specified entitiy
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="entity"/> is null</exception>
        public void Delete(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            _objectSet.DeleteObject(entity);
        }

        /// <summary>
        /// Deletes records matching the specified criteria
        /// </summary>
        /// <param name="predicate">Criteria to match on</param>
        public void Delete(Func<T, bool> predicate)
        {
            IEnumerable<T> records = from x in _objectSet.Where<T>(predicate) select x;

            foreach (T record in records)
            {
                _objectSet.DeleteObject(record);
            }
        }

        /// <summary>
        /// Exclui a entidade especificada
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="entity"/> is null</exception>
        public virtual void Exclui(T entity)
        {
            this.Delete(entity);
            this.SaveChanges();
        }

        /// <summary>
        /// Exclui registros que atendam ao critério especificado
        /// </summary>
        /// <param name="predicate">Criteria to match on</param>
        public virtual void Exclui(Func<T, bool> predicate)
        {
            this.Delete(predicate);
            this.SaveChanges();
        }


        /// <summary>
        /// Adds the specified entity
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="entity"/> is null</exception>
        public void Add(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            _objectSet.AddObject(entity);            
        }

        /// <summary>
        /// Inclui a entidade informada e persiste no banco
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="entity"/> is null</exception>
        public virtual void Inclui(T entity)
        {
            this.Add(entity);
            this.SaveChanges();
        }

        /// <summary>
        /// Attaches the specified entity
        /// </summary>
        /// <param name="entity">Entity to attach</param>
        public void Attach(T entity)
        {
            _objectSet.Attach(entity);
        }

        /// <summary>
        /// Detaches the specified entity
        /// </summary>
        /// <param name="entity">Entity to detach</param>
        public void Detach(T entity)
        {
            _objectSet.Detach(entity);
        }

        /// <summary>
        /// Saves all context changes
        /// </summary>
        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        /// <summary>
        /// Saves all context changes with the specified SaveOptions
        /// </summary>
        /// <param name="options">Options for saving the context</param>
        public void SaveChanges(SaveOptions options)
        {
            _context.SaveChanges(options);
        }

        /// <summary>
        /// Releases all resources used by the WarrantManagement.DataExtract.Dal.ReportDataBase
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the WarrantManagement.DataExtract.Dal.ReportDataBase
        /// </summary>
        /// <param name="disposing">A boolean value indicating whether or not to dispose managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_context != null)
                {
                    _context.Dispose();
                    _context = null;
                }
            }
        }

        /// Retorna o nome do EntitySet, possibilitando a criação de métodos genéricos.
        private string GetEntitySetName(string entityTypeName)
        {
            var container = _context.MetadataWorkspace
                                            .GetEntityContainer(_context.DefaultContainerName,
                                                                       DataSpace.CSpace);
            string entitySetName = (from meta in container.BaseEntitySets
                                    where meta.ElementType.Name == entityTypeName
                                    select meta.Name).FirstOrDefault();

            return entitySetName;
        }

        /// Atualiza um objeto existente.
        public virtual void Update(T entity)
        {            
            EntityKey key;
            object originalItem;

            if (entity.EntityKey == null)
                // Obtém o entity key do objeto que será atualizado
                key = Context.CreateEntityKey(EntitySetName, entity);
            else
                key = entity.EntityKey;
            try
            {
                // Obtém o objeto original
                if (Context.TryGetObjectByKey(key, out originalItem))
                {
                    if (originalItem is EntityObject &&
                        ((EntityObject)originalItem).EntityState != EntityState.Added)
                    {
                        // Autaliza o objeto
                        //context.ApplyPropertyChanges(key.EntitySetName, entity);
                        _context.ApplyCurrentValues<T>(key.EntitySetName, entity);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// Atualiza um objeto existente.
        public virtual void Altera(T entity)
        {
            this.Update(entity);
            this.SaveChanges();
        }
    }
}
