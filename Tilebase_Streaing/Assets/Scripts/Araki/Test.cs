using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Test : MonoBehaviour
{
    public int n = 1;
    // Start is called before the first frame update
    async void Start()
    {
        await TestTask();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("klfahgihaiognlkdnaklnklf");
        }
    }

    // ��Ƀo�b�N�O���E���h�ōs������
    async UniTask TestTask()
    {
        var token = this.GetCancellationTokenOnDestroy();
        while (true)
        {
            Debug.Log("aaaaaaaaa");
            await UniTask.Delay(1000, cancellationToken: token);
        }
    }
}
