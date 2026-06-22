// USING LINES:
// | Keep using lines at the top of your file.
// | Always remove unused lines.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// NAMESPACES:
// | Use namespaces to ensure that your classes, interfaces, enums, won’t conflict with
// | existing ones from other namespaces or the global namespace.
// | Use Pascal case, without special symbols or underscores.
// | Create sub-namespaces with the dot (.) operator, e.g. MyApplication.GameFlow, MyApplication.AI, etc.

namespace CSharp.StyleGuide.Example
{
    // ENUMS:
    // | Use Pascal case for enum names and values.
    // | Use a singular noun for the enum name as it represents a single value from a set of possible values. 
    // | They should have no prefix or suffix.
    // | Prefer to create a separate file for each enum. But you can place public enums outside of a class to make them global.
    public enum Direction
    {
        North,
        South,
        East,
        West,
    }

    // FLAG ENUM:
    // | Use flag enum to represent combinations of options when multiple values can be chosen at the same time, enabling bitwise operations.
    // | Use a plural noun to indicate the possibility of multiple selections (e.g., AttackModes). 
    // | Use column-alignment for binary values.
    // | Alternatively, consider using the 1 << bitnum style.
    [Flags]
    public enum AttackModes
    {
        // Decimal                         // Binary
        None = 0,                          // 000000
        Melee = 1,                         // 000001
        Ranged = 2,                        // 000010
        Special = 4,                       // 000100

        MeleeAndSpecial = Melee | Special  // 000101
    }

    // INTERFACES:
    // | Interfaces allow you to define a common contract, when unrelated classes need to share common functionality but implement it differently
    // | Prefix interface names with a capital I
    // | Follow this with naming interfaces with adjective phrases that describe the functionality.
    public interface IDamageable
    {
        string DamageTypeName { get; }
        float DamageValue { get; }

        void ApplyDamage(string description, float damage, int numberOfHits);
    }
    
    // CLASSES or STRUCTS:
    // | Use Pascal case and avoid prefixes
    // | Name them with nouns or noun phrases. This distinguishes type names from methods, which are named with verb phrases.
    // | One Monobehaviour per file. If you have a Monobehaviour in a file, the source file name must match. 
    public class StyleExample : MonoBehaviour
    {
        // TODO: add local functions with local variables example
        
        // EVENTS:
        // | Name with a verb phrase.
        // | Present participle means "before" and past participle means "after."
        // | Choose a naming scheme for events, event handling methods (subscriber/observer), and event raising methods (publisher/subject)
        // | e.g. event/action = "OpeningDoor", event raising method = "OnDoorOpened", event handling method = "HandleDoorOpened"
   
        // Event before
        public event Action OnOpeningDoor;

        // Event after
        public event Action OnDoorOpened;     

        // Event with int parameter
        public event Action<int> PointsScored;
        
        // FIELDS: 
        // | Avoid special characters (backslashes, symbols, Unicode characters); these can interfere with command line tools.
        // | Use [SerializeField] attribute if you want to display a private field in Inspector.
        [SerializeField] private bool _isPlayerDead;
        
        // Use the Range attribute to set minimum and maximum values. 
        // This limits the values to a Range and creates a slider in the Inspector.
        [Range(0f, 1f)] 
        [SerializeField] float _rangedStat;

        // A tooltip can replace a comment on a serialized field and do double duty.
        [Tooltip("This is another statistic for the player.")]
        [SerializeField] float _anotherStat;
        
        private const int MaxCount = 100;
        
        private static readonly DefaultPath = "Path/MyUser/PathFolder";
        private static GameManager _instance; 
        
        // Always leave class fields as private
        // To enforce encapsulation we should always leave a class field as private. If you need to expose it at any level you can create a protected/public property for it.
        // Structs can have public variables, as they are immutable already and are meant to represent values.
        private int _elapsedTimeInHours;

        // PROPERTIES:
        // | Use the expression-bodied properties to shorten, but choose your preferred format.
        // | E.g. use expression-bodied for read-only properties but { get; set; } for everything else.
        // | Use the Auto-Implementated Property for a public property without a backing field.
        // | For get or set operations involving complex logic or computation, use methods instead of properties.

        // The private backing field
        private int _maxHealth;

        // Read-only, returns backing field
        public int MaxHealth => _maxHealth;

        // Equivalent to: 
        public int MaxHealth { get; private set; }

        // Explicitly implementing getter and setter
        public int MaxHealth
        {
            get => m_maxHealth;
            set => m_maxHealth = value;
        }
        
        // | Auto-implemented property without backing field
        public string MaxHealth { get; set; }

        // | Always unsubscribe from events
        // | Event publishers hold strong references to subscribers.
        // | If a subscriber is destroyed without unsubscribing, it won't be garbage collected.
		private void OnEnable()
		{
    		GameManager.OnGameStarted += HandleGameStarted;
		}

        private void OnDisable()
        {
            GameManager.OnGameStarted -= HandleGameStarted;
        }
        
        // | These are event subscription handling methods, e.g. OnDoorOpened, OnPointsScored, etc.
        // | Prefix the event handling method with “Handle”.
        public void HandleGameStarted()
        {
            // ..
        }
        
        // METHODS:
        // | While “function” and “method” are often used interchangeably, method is the right term in Unity development
        // | because you can’t write a function without incorporating it into a class in C#.
        // | Start a method name with a verb or verb phrases to show an action. Add context if necessary. e.g. GetDirection, FindTarget, etc.
        // | Methods returning bool should ask questions: Much like Boolean variables themselves.
        // | Use camel case for parameters. Format parameters passed into the method like local variables.
        // | Avoid long methods. If a method is too long, consider breaking it into smaller methods.
        // | Avoid methods with too many parameters. If a method has more than four parameters, 
        // | consider using a class or struct to group them.
        // | Avoid excessive overloading: You can generate an endless permutation of method overloads.
        // | Avoid side effects: A method only needs to do what its name advertises.
        // | A good name for a method reflects what it does.
        // | Avoid setting up methods to work in multiple different modes based on a flag. 
        // | Make two methods with distinct names instead, e.g. GetAngleInDegrees and GetAngleInRadians. 
        public void SetInitialPosition(float x, float y, float z)
        {
            transform.position = new Vector3(x, y, z);
        }

        public bool IsNewPosition(Vector3 newPosition)
        {
            return (transform.position == newPosition);
        }

        private void FormatExamples(int someExpression)
        {
            // only use VAR when:
            // | The type is clear and explicit from the right-hand side,
            // | When the explicit type would be overly long/verbose
            // | In foreach loops, var ensures that the iteration variable matches the type provided by the enumerator. 
            // | If you explicitly declare a mismatched type, the compiler may allow it, leading to runtime errors so be careful.
            var powerUps = new List<PlayerStats>();
            var gameObjectsFromName = new Dictionary<string, List<GameObject>>();

            foreach (var item in gameObjectsFromName)
            {
                // ..
            }

            // SWITCH STATEMENTS:
            // | It’s generally advisable to replace longer if-else chains with a switch statement for better readability.
            switch (someExpression)
            {
                case 0:
                    // ..
                    break;
                case 1:
                    // ..
                    break;
                case 2:
                    // ..
                    break;
            }

            // BRACES: 
            // | Do not omit braces and avoid single-line statements for readabiliy and debuggability.
            // | Keep braces in nested multi-line statements.

            // this works
            for (int i = 0; i < 100; i++) { DoSomething(i); }

            // but this is more readable and often more debuggable. 
            for (int i = 0; i < 100; i++)
            {
                DoSomething(i);
            }

            // | Separate the statements for readability.
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    DoSomething(j);
                }
            }
        }
    }

    // OTHER CLASSES:
    // | Avoid multiple types on the same file
    // | Exceptions apply for nested types but be careful to not bloat the code with many nested types on the same class or struct. 
    // | If you have a huge amount of nested types due to some big internal logic
    // | you may want to consider moving all that stuff into a different namespace and/or assembly.
    [Serializable]
    public struct PlayerStats
    {
        public int MovementSpeed;
        public int HitPoints;
        public bool HasHealthPotion;
    }
}
