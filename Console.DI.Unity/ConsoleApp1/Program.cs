using System;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            StatrUp.RegisterType();

            var BMW = StatrUp.container.Resolve<ICar>(); // 解析物件 default
            var Ford = StatrUp.container.Resolve<ICar>("FordCar"); // 具名解析 Ford
            var Audi = StatrUp.container.Resolve<ICar>("AudiCar"); // 具名解析 Audi

            BMW.Run();
            Ford.Run();
            Audi.Run();

            var MyDriverFordCar = StatrUp.container.Resolve<Driver>("MyDriverFordCar");
            MyDriverFordCar.RunCar();

            var MyDriverBMW = StatrUp.container.Resolve<Driver>(); //如果不給的話預設會抓 BMW，因為 ICar 預設註冊的是 BMW
            MyDriverBMW.RunCar();

            var PropertyDI = StatrUp.container.Resolve<Driver2>();
            PropertyDI.RunCar();

            var MethodDI = StatrUp.container.Resolve<Driver3>();
            MethodDI.RunCar();

            Console.Read();
        }
    }

    public static class StatrUp
    {
        /// <summary>
        /// 建立靜態 DI 容器
        /// </summary>
        public static IUnityContainer container { get; private set; }

        /// <summary>
        /// 註冊物件
        /// </summary>
        public static void RegisterType()
        {
            container = new UnityContainer();
            container.RegisterType<ICar, BMW>(new PerResolveLifetimeManager());  //default
            container.RegisterType<ICar, Ford>("FordCar"); //具名註冊
            container.RegisterType<ICar, Audi>("AudiCar"); //具名註冊

            container.RegisterType<ICarKey, AudiKey>();

            //如果註冊一個物件當建構式需要物件時，可以使永 InjectionConstructor，然後並請告訴她一個預設的 DI 物件，
            //所以當在解析 Driver 時程式會知道 Driver  建構式 需要注入的是誰         
            container.RegisterType<Driver>("MyDriverFordCar",
                new InjectionConstructor(container.Resolve<ICar>("FordCar"),
                                         container.Resolve<ICarKey>())); //預設 ICarKey 會是 AudiKey
        }

        // DI 物件 生命週期管理
        // TransientLifetimeManager：每次調用Resolve或ResolveAll方法時，都會創建一個請求類型的新對象。
        // ContainerControlledLifetimeManager：首次調用Resolve或ResolveAll方法時創建一個單例對象，然後在後續的Resolve或ResolveAll調用中返回相同的對象。
        // HierarchicalLifetimeManager：與ContainerControlledLifetimeManager相同，唯一的區別是子容器可以創建自己的單例對象。父容器和子容器不共享相同的單例對象。
        // PerResolveLifetimeManager：與TransientLifetimeManager相似，但是它在遞歸對像圖中重用了註冊類型相同的對象。
        // PerThreadLifetimeManager：每個線程創建一個單例對象。它從不同線程上的容器返回不同的對象。
        // ExternallyControlledLifetimeManager：當您調用Resolve或ResolveAll方法時，它僅維護對其創建的對象的弱引用。它不維護其創建的強對象的生存期，並允許您或垃圾收集器控制對象的生存期。它使您可以創建自己的自定義生命週期管理器。
    }

    /// <summary>
    /// 建構式注入示範
    /// </summary>
    public class Driver
    {
        private ICar _car = null;
        private ICarKey _key = null;

        //如果一個類別有多個建構式，想要使用DI 的建構式 就必須掛上 [InjectionConstructor] 
        [InjectionConstructor]
        public Driver(ICar car, ICarKey key)
        {
            _car = car;
            _key = key;
        }

        public void RunCar()
        {
            _car.Run();
        }
    }

    /// <summary>
    /// 屬性注入示範
    /// </summary>
    public class Driver2
    {
        [Dependency()] //告知DI 容器 這個屬性要被注入，設設會取得 BMW
        public ICar Car { get; set; }

        [Dependency("FordCar")]  //告知DI 容器 這個屬性要被注入 會取得FordCar
        public ICar Car2 { get; set; }

        public void RunCar()
        {
            Car.Run();
            Car2.Run();
        }
    }

    /// <summary>
    /// 方法注入示範
    /// </summary>
    public class Driver3
    {
        private ICar _car = null;

        [InjectionMethod] // 告訴DI 容器，這個方法的 ICar 是需要被注入的，當 Driver3 被建立起來後，會跑這個方法
        public void UseCar(ICar car)
        {
            _car = car;
        }

        public void RunCar()
        {
            _car.Run();
        }
    }
}
