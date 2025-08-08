using UnityEngine;

/// <summary>
/// Wind Element Test - Wind element'ini test etmek için kullanılır
/// </summary>
public class WindElementTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private GameObject testTarget;
    [SerializeField] private KeyCode testKey = KeyCode.W;
    [SerializeField] private KeyCode addStackKey = KeyCode.E;
    [SerializeField] private KeyCode removeStackKey = KeyCode.R;
    
    private WindElement windElement;
    private ElementStack elementStack;
    
    private void Start()
    {
        windElement = new WindElement();
        
        if (testTarget == null)
        {
            // Find an enemy to test with
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemies.Length > 0)
            {
                testTarget = enemies[0];
            }
        }
        
        if (testTarget != null)
        {
            elementStack = testTarget.GetComponent<ElementStack>();
            if (elementStack == null)
            {
                elementStack = testTarget.AddComponent<ElementStack>();
            }
        }
        
        Debug.Log("Wind Element Test initialized. Press W to test wind effect, E to add stack, R to remove stack");
    }
    
    private void Update()
    {
        if (testTarget == null || elementStack == null) return;
        
        if (Input.GetKeyDown(testKey))
        {
            TestWindEffect();
        }
        
        if (Input.GetKeyDown(addStackKey))
        {
            AddWindStack();
        }
        
        if (Input.GetKeyDown(removeStackKey))
        {
            RemoveWindStack();
        }
    }
    
    private void TestWindEffect()
    {
        Debug.Log("🧪 Testing Wind Effect...");
        
        // Add a wind stack
        elementStack.AddElementStack(ElementType.Wind, 1);
        
        // Get current stacks
        int currentStacks = elementStack.GetElementStack(ElementType.Wind);
        Debug.Log($"🧪 Current wind stacks: {currentStacks}");
        
        // Trigger wind effect
        windElement.TriggerElementEffect(testTarget, currentStacks);
    }
    
    private void AddWindStack()
    {
        elementStack.AddElementStack(ElementType.Wind, 1);
        int currentStacks = elementStack.GetElementStack(ElementType.Wind);
        Debug.Log($"🧪 Added wind stack. Current stacks: {currentStacks}");
    }
    
    private void RemoveWindStack()
    {
        elementStack.RemoveElementStack(ElementType.Wind, 1);
        int currentStacks = elementStack.GetElementStack(ElementType.Wind);
        Debug.Log($"🧪 Removed wind stack. Current stacks: {currentStacks}");
    }
} 