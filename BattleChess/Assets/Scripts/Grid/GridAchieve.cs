using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static GridSettings;

//[ExecuteInEditMode]
//让脚本在没有启动的情况下也发挥作用
public class GridAchieve : MonoBehaviour
{
    public static GridAchieve instance;
    [Header("地块生成")]
    [SerializeField] private Transform gridPrefab;
    [SerializeField] private Transform parentObj;
    [SerializeField] private GameObject gridSample;
    [SerializeField] private bool placenmentMode = false;
    Dictionary<Vector3Int, GridSettings> allGridPos = new Dictionary<Vector3Int, GridSettings>();
    public Grid grid;
    private void Start()
    {
        grid = GetComponent<Grid>();
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        PopulateGridDictionary();//确保字典里有值
        //DictionaryValueCheck();
    }
 private void Update()
 {
     UnityEngine.Vector3 selectedPosition = MouseWorld.GetPosition();
     Vector3Int cellPosition = grid.WorldToCell(selectedPosition);
     //将世界坐标换成网格坐标
     if (placenmentMode)
     {
         gridSample.transform.position = grid.GetCellCenterWorld(cellPosition);
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (allGridPos.ContainsKey(cellPosition))
                {
                    Debug.LogError("There is already have one!");
                }
                else
                {
                    CreateGrid(cellPosition);
                }
            }

     }
 }

    private void CreateGrid(Vector3Int cellPosition)
    {
        Transform insGrid = Instantiate(gridPrefab, gridSample.transform.position, UnityEngine.Quaternion.identity, parentObj);
        GridSettings gridSettings = insGrid.GetComponent<GridSettings>();
        gridSettings.gridCellPosition = cellPosition;
        //创建地图格的时候就顺便把当前坐标传给对应的GridSettings脚本，这样每个地块就有坐标了
        allGridPos.Add(cellPosition,gridSettings);
    }

    public UnityEngine.Vector3 TranslatePosIntoGridPos(UnityEngine.Vector3 unchangedPos) 
    {
        return grid.WorldToCell(unchangedPos);
    }

    public List<Vector3Int> PathFind(UnityEngine.Vector3 finalPos)
    {
        Vector3Int startPos = UnitActionSystem.Instance.selectedUnit.GetGroundGrid().gridCellPosition;
        Vector3Int endPos = Vector3Int.FloorToInt(TranslatePosIntoGridPos(finalPos));
        //Debug.Log($"终点坐标转换结果：{endPos}" + endPos.GetType());
        // 检查目标位置是否存在网格
        if (allGridPos.ContainsKey(endPos))
        {
            // A*寻路
            List<Vector3Int> path = FindPath(startPos, endPos);
            //Debug.Log($"生成的路径长度：{path?.Count ?? 0}");
            return path;
        }
        else
        {
            Debug.LogError("Target grid does not exist!");
            return null;
        }
    }

    private List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int endPos)
    {
        List<Node> openList = new List<Node>();
        //用于存储待探索的节点
        HashSet<Node> closedList = new HashSet<Node>();
        //用于存储已经探索过的节点，防止重复
        //HashSet会把值转化成哈希码以便存储，在没有重写GetHashCode的情况下，哈希码根据存储位置不同而不同，导致坐标等完全相同的实例不被认为是相等的，从而无限循环

        Node startNode = new Node(startPos, null, 0, GetManhattanDistance(startPos, endPos));
        //给起点地块赋予类实例，本来就在起点所以gCost为0
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // 找到开放列表中fCost最小的节点
            Node currentNode = GetLowestFCostNode(openList);
            //Debug.Log(currentNode?.parent?.position);
           // Debug.Log($"当前节点：{currentNode.position} (g={currentNode.gCost}, h={currentNode.hCost})");
            openList.Remove(currentNode);
            closedList.Add(currentNode);
            //依次循环访问当前地块周围的4格地块，将openList清空以便下一个节点使用，并加入closedList来表示已被探索

            // 如果当前节点是目标节点，回溯路径
            if (currentNode.position == endPos)
            {
                Debug.Log("找到路径！");
                return RetracePath(startNode, currentNode);
            }

            // 获取当前节点的相邻节点
            List<Node> neighbors = GetNeighbors(currentNode);
            //在访问起点地块后，上方openList已经空了(最开始只有一个地块在List<Node> openList中)，所以马上获取周围地块
            foreach (Node neighbor in neighbors)
            {
                if (closedList.Contains(neighbor))
                //已访问过的不管
                {
                    continue;
                }

                int newGCost = currentNode.gCost + GetMoveCost(neighbor.position);
                //gCost是当前节点到起点的代价，实际是从起点走到当前这个点所用的移动点
                if (newGCost < neighbor.gCost || !openList.Contains(neighbor))
                //更新每个相邻地块的gCost和hCost(因为GetNeighbors()方法中默认设了0)，newGCost < neighbor.gCost这个条件不是很理解，怎么会有不重复的多走一步代价却更小的情况呢
                {
                    neighbor.gCost = newGCost;
                    neighbor.hCost = GetManhattanDistance(neighbor.position, endPos);
                    neighbor.parent = currentNode;
                    //不管选走哪个地块，都是由上一个的最优地块走到的，所以无论如何都是上一个最优地块作为父物体

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }
        // 没有找到路径
        return null;
    }

    private Node GetLowestFCostNode(List<Node> nodeList)
        //遍历访问列表中的每个地块，返回fCost最小的
    {
        Node lowestFCostNode = nodeList[0];
        for (int i = 1; i < nodeList.Count; i++)
        {
            if (nodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = nodeList[i];
            }
        }
        return lowestFCostNode;
    }

    private List<Node> GetNeighbors(Node node)
        //游戏卡死的原因在于在本方法中获取到的四个相邻地块算是新的实例，即使值相同，没有重写Equals方法之前也算做不同的实例
        //导致的问题是已经走过的路因为新实例的生成而被重新走，走过之后又生成相同的实例走回去，无限循环导致了卡死
    {
        List<Node> neighbors = new List<Node>();
        Vector3Int[] directions = { Vector3Int.forward, Vector3Int.back, Vector3Int.left, Vector3Int.right };

        foreach (Vector3Int direction in directions)
        {
            Vector3Int neighborPos = node.position + direction;
            //Vector3Int[]里全是单位向量，在本地块的坐标上+1就是相邻地块的坐标了，allGridPos有所有地块的坐标集，找得到就在通过值和键的对应找到对应的GridSettings实例
            if (allGridPos.TryGetValue(neighborPos, out GridSettings neighborGrid) && !neighborGrid.occupied)
                //TryGetValue尝试获取与指定键关联的值，而不会像直接通过索引访问那样在键不存在时抛出异常
                //这里一个方法将下面两个语句合在一起了                
                //allGridPos.ContainsKey(neighborPos)
                //GridSettings neighborGrid = allGridPos[neighborPos];
            {
                //访问周围四个地块，将没有被占据(能移动的)地块加入list，当前为中心的最佳地块就成了父物体，周围四个中的最佳地块会成为中心地块的子物体
                bool isPassable = neighborGrid.gridType != GridType.hinderGrid && !neighborGrid.occupied;
                //把阻挡的地块排除
                if (isPassable)
                {
                    Debug.Log($"有效邻居：{neighborPos}");
                    neighbors.Add(new Node(neighborPos, null, 0, 0));
                }
            }
        }

        return neighbors;
    }

    private List<Vector3Int> RetracePath(Node startNode, Node endNode)
        //返回的list就是从起点到终点的最短路径的地块坐标的集合
    {
        //在一个节点找到一个最佳地块之后，下一次会遍历上一个最佳地块周围的四个地块(自动排除已探索的)，
        //找到最佳地块之后存储其信息并将其设为上一个最佳地块的子物体，信息链在一起，以此实现回溯路径
        List<Vector3Int> path = new List<Vector3Int>();
        Node currentNode = endNode;

        while (currentNode != startNode)
            //在回溯到起点之前一直找最佳地块的父物体，并将他们的坐标付出去
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        //Reverse 方法用于反转 List<T> 中元素的顺序
        //因为你回溯出的list是倒着走的

        return path;
    }

    private int GetMoveCost(Vector3Int position)
        //传坐标进去，字典里找到GridSettings实例，通过实例访问地块的移动消耗并返回
    {
        if (allGridPos.ContainsKey(position))
        {
            GridSettings grid = allGridPos[position];
            return grid.GridMoveInformation();
        }
        return 0;
    }

    private int GetManhattanDistance(Vector3Int startPos, Vector3Int endPos)
    {
        return Mathf.Abs(startPos.x - endPos.x) + Mathf.Abs(startPos.z - endPos.z);
        //Abs求绝对值的，求出曼哈顿距离
    }

    private class Node
    {
        //A*寻路找最佳路径要在每个节点把周围的地块到起点和到终点的距离进行比较，所以有创建构造类的必要，每个地块一个类实例，便于访问管理
        public Vector3Int position;
        public Node parent;
        public int gCost;
        //从起点到当前节点的实际距离
        public int hCost;
        //从终点到当前节点的实际距离
        public int fCost => gCost + hCost;

        public Node(Vector3Int position, Node parent, int gCost, int hCost)
        {
            this.position = position;
            this.parent = parent;
            this.gCost = gCost;
            this.hCost = hCost;
        }

        public override bool Equals(object obj)
        //重写Equals时，在此后所有Node进行比较的情况都会默认调用这个Equals，没有显式调用，但必不可少
        {
            if (obj is Node other)
                return position.Equals(other.position);
            return false;
        }
        public override int GetHashCode() => position.GetHashCode();
        //作用同上 根据 position 生成唯一的哈希码，确保相同坐标的节点在哈希表中被归类到同一位置，确保HashCode唯一性
    }
    private void PopulateGridDictionary()
    //我的地图是直接在game里做好然后复制到scene中去的，在生成时allGridPos会有值，但下次启动就没值了，所以每次开始时都重新赋一次
    //ExecuteInEditMode不会保存变量值吗？？
    {
        // 查找场景中的所有GridSettings组件
        GridSettings[] allGridSettings = FindObjectsOfType<GridSettings>();

        foreach (GridSettings gridSetting in allGridSettings)
        {
            // 获取GridSettings实例的坐标
            Vector3Int position = gridSetting.gridCellPosition;

            // 检查字典中是否已经包含该坐标
            if (!allGridPos.ContainsKey(position))
            {
                // 如果不包含，则将其添加到字典中
                allGridPos.Add(position, gridSetting);
            }
        }
    }
    private void DictionaryValueCheck()
        //检测字典里到底有没有值，有什么值
    {
        foreach (KeyValuePair<Vector3Int, GridSettings> pair in allGridPos)
        {
            Vector3Int key = pair.Key;
            GridSettings value = pair.Value;

            Debug.Log($"Key: {key}, GridType: {value.gridType}, Occupied: {value.occupied}");
        }
    }
}
