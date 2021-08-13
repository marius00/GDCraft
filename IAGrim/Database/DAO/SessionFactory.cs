using System;
using NHibernate;
using log4net;
using System.Threading;

namespace IAGrim.Database {
    public interface ISessionCreator {

        ISession OpenSession();

        IStatelessSession OpenStatelessSession();
    }

    public class SessionFactory : ISessionCreator {
        private static ILog logger = LogManager.GetLogger(typeof(SessionFactory));

        [ThreadStatic]
        private static SessionFactoryLoader.SessionFactory sessionFactory;

        static SessionFactory() {
            System.Net.ServicePointManager.Expect100Continue = false;
        }

        public ISession OpenSession() {
            if (sessionFactory == null) {
                sessionFactory = new SessionFactoryLoader.SessionFactory();
                logger.Info($"Creating session on thread {Thread.CurrentThread.ManagedThreadId}");
            }

            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "NH:Session";

            //logger.DebugFormat("Session opened on thread {0}, Stacktrace: {1}", System.Threading.Thread.CurrentThread.Name, new System.Diagnostics.StackTrace());
            return sessionFactory.OpenSession();
        }

        public IStatelessSession OpenStatelessSession() {
            if (sessionFactory == null) {
                sessionFactory = new SessionFactoryLoader.SessionFactory();
                logger.Info($"Creating session on thread {Thread.CurrentThread.ManagedThreadId}");
            }

            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "NH:Session";

            //logger.DebugFormat("Stateless session opened on thread {0}, Stacktrace: {1}", System.Threading.Thread.CurrentThread.Name, new System.Diagnostics.StackTrace());
            return sessionFactory.OpenStatelessSession();
        }
    }
}
