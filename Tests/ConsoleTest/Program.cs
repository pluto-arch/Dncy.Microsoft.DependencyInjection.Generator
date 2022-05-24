using System;
using ConsoleTest.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleTest
{


    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AutoInjectConsoleTest();
            var provider = services.BuildServiceProvider();

            var demoSer = provider.GetRequiredService<Demo>();
            demoSer.Cw();


            var userRep = provider.GetRequiredService<IUserRepository>();
            Console.WriteLine(userRep.Get());


            var serviecA = provider.GetRequiredService<IServiceA>();
            serviecA.Cw();


            var gen = provider.GetRequiredService<IBase<User>>();
            var dd = gen.Get();
            Console.WriteLine(dd.Name);


            var gen2 = provider.GetRequiredService<IBase2<User,Per>>();
            var dd2 = gen2.Get();


            Console.WriteLine("Hello");
            Console.ReadKey();
        }
    }

    [Injectable(InjectLifeTime.Scoped)]
    public class Demo
    {
        public void Cw()
        {
            Console.WriteLine("Demo");
        }
    }


    public interface IServiceA
    {
        void Cw();
    }


    [Injectable(InjectLifeTime.Scoped, typeof(IServiceA))]
    public class ServiceA:IServiceA
    {
        public void Cw()
        {
            Console.WriteLine("IServiceA");
        }
    }



    public class User
    {
        public string Name { get; set; }
    }

    public class Per
    {
        public string Name { get; set; }
    }


    public interface IBase<T> 
        where T : class
    {
        T Get();
    }


    [Injectable(InjectLifeTime.Scoped,typeof(IBase<>))]
    public class Base<T> : IBase<T> where T:class
    {
        /// <inheritdoc />
        public T Get()
        {
            return default;
        }
    }


    [Injectable(InjectLifeTime.Scoped,typeof(IBase<User>))]
    public class BaseB : IBase<User>
    {
        /// <inheritdoc />
        public User Get()
        {
            return new User(){Name = "12312"};
        }
    }



    public interface IBase2<T1,T2> 
        where T1 : class
        where T2 : class
    {
        string Get();
    }

    [Injectable(InjectLifeTime.Transient,typeof(IBase2<,>))]
    public class Base2sss<T1,T2>: IBase2<T1,T2> 
        where T1 : class
        where T2 : class
    {
        public string Get()
        {
            return "123123123";
        }
    }

}