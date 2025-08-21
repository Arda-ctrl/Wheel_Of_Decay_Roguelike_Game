using UnityEngine;

// Bu dosya Unity'nin scripts'i yeniden derlemesini sağlamak için kullanılıyor
public class ForceRecompile : MonoBehaviour
{
    // Bu değişken özellikle her Unity başlangıcında yeniden derleme yapmak için kullanılıyor
    public static int ForceCompilationIteration = 0;
    
    void Start()
    {
        ForceCompilationIteration++;
    }
}

