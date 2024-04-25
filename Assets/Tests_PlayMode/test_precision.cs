using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class test_precision
{
    [SetUp]
    public void Setup()
    {   
        SceneManager.LoadScene(3);
        SceneFlowManager[] components = GameObject.FindObjectsOfType<SceneFlowManager>();
        /*foreach (SceneFlowManager component in components)
        {
            component.SetDev();
        }*/
    }

    [UnityTest]
    public IEnumerator test_precisionWithEnumeratorPasses()
    {
        //ARRANGE
        yield return new WaitUntil(() => SceneManager.GetActiveScene().buildIndex == 3);
        yield return new WaitForFixedUpdate();
        //ACT
        TimeManager.i.source.Play();
        TimeManager.i.source.volume = 0;
        TimeManager.i.source.time = 7;
        yield return new WaitForFixedUpdate();
        //ASSERT
        yield return new WaitForFixedUpdate();
    }
}
