using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  abstract class MovingObject : MonoBehaviour
{
    public float moveTime = .1f;
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    private float inverseMoveTime;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1 / moveTime;
    }

    protected bool Move(int xDir,int yDir,out RaycastHit2D hit)
    {
        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);

        boxCollider.enabled = false;
        hit = Physics2D.Linecast(start, end, blockingLayer);
        boxCollider.enabled = true;

        if(hit.transform == null)
        {
            StartCoroutine(SmoothMovement(end));
            return true;
        }
        return false;
    }

    protected IEnumerator SmoothMovement(Vector3 end)
    {
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            //                                        起点           终点             速度 * 帧间时间       此函数返回一个从起点向终点方向移动了速度 * 帧间时间的距离后得到的position
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
            rb2D.MovePosition(newPosition);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            //表示剩余代码将在下一帧继续执行。也就是说代码每次进入while循环读到yield return null之后会暂停执行，下一帧再回来进行下一次循环
            yield return null;
        }
    }

    protected virtual void AttempMove<T>(int xDir, int yDir)where T:Component
    {
        
        if (xDir == -1)
            transform.localScale = new Vector3(-1,1,1);
        else if(xDir == 1)
            transform.localScale = new Vector3(1, 1, 1);

        RaycastHit2D hit;
        //希望函数可以返回一个以上的数据时，可以用out关键字，实际上就是C++的指针，hit传入的不是一个值而是自己的内存地址
        bool canMove = Move(xDir, yDir, out hit);

        if (hit.transform == null)
            return;

        T hitComponent = hit.transform.GetComponent<T>();

        // if (!canMove && hit.transform != null)不能这么写，因为尽管hit.transform不是null，但是上一步的hitComponent因为类型不匹配所以hitComponent为null，要以hitComponent为判断标准
        //if (!canMove && hitComponent != null)
        if (!canMove)
            OnCantMove(hitComponent);
    }

    protected abstract void OnCantMove<T>(T component) where T : Component;

}
