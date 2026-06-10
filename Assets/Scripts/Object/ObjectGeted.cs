using System.Collections;
using UnityEngine;

/// <summary>
/// 조사 시 아이템을 획득하는 자식 클래스.
/// ObjectTrigger를 상속하며, 조사 완료 후 아이템 획득 UI를 표시하고
/// 아무 키를 누르면 닫힌다. 한 번 획득하면 이후 조사 불가.
/// </summary>
public class ObjectGeted : ObjectTrigger
{
    [Header("아이템 설정")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private GameObject itemGetUI; // 아이템 획득 UI (태그: ItemGetUI)
    [SerializeField] private GameObject alreadyGetUI;
    private bool isItemTaken = false; // 한 번 획득하면 true
    

    protected override void Start()
    {
        base.Start();

        if (itemGetUI == null)
            itemGetUI = GameObject.FindWithTag("ItemGetUI");

        if (itemGetUI != null)
            itemGetUI.SetActive(false);

        if(alreadyGetUI != null)
            alreadyGetUI.SetActive(false);
    }

    protected override void Update()
    {
 

        base.Update();
    }

    protected override void OnInspectComplete()
    {
        IsShowingUI = true;

        if (isItemTaken)
        {
            CoroutineGetUI();
            return;
        }
        if (itemData != null)
            Inventory.Instance.AddItem(itemData);

        


        Vector3 center = detectionCenter != null ? detectionCenter.position : transform.position;
        Vector3 uiPos = center + uiOffset;
        if (itemGetUI != null)
        {
            itemGetUI.transform.position = uiPos;
            itemGetUI.SetActive(true);
        }
            

        isItemTaken = true;
    }


    protected override void OnAnyKeyWhileUI()
    {
        IsShowingUI = false;
        itemGetUI?.SetActive(false);
        alreadyGetUI?.SetActive(false);
        // alreadyGet 코루틴도 중단
        StopCoroutine(nameof(OpenAlreadyGetUI));
    }
    private void CoroutineGetUI()
    {
        StartCoroutine(OpenAlreadyGetUI());
    }
    IEnumerator OpenAlreadyGetUI()
    {
        Vector3 center = detectionCenter != null ? detectionCenter.position : transform.position;
        Vector3 uiPos = center + uiOffset;
        if (alreadyGetUI != null)
        {
            alreadyGetUI.transform.position = uiPos;
            alreadyGetUI.SetActive(true);
        }
        yield return new WaitForSeconds(1.5f);
        alreadyGetUI.SetActive(false);
        IsShowingUI = false;
    }
}