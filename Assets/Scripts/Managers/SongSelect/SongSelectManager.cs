using Lean.Pool;
using Nova;
using NovaSamples.Effects;
using Riptide;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SongSelectManager : MonoBehaviour
{
    [Header("Références")]
    [SerializeField]
    private UIBlock2D scroller;

    [SerializeField]
#pragma warning disable CS0108
    private Camera camera;
#pragma warning restore CS0108

    [SerializeField]
    private ListView mapList;

    [SerializeField]
    private UIBlock2D uiRoot;

    [SerializeField]
    private Transform arrierePlansParent;

    [SerializeField] private TextBlock hotePlayer;
    [SerializeField] private TextBlock toursRestants;

    private KeyCode MOVE_LEFT;
    private KeyCode MOVE_RIGHT;
    private KeyCode RANDOM_MAP;
    private KeyCode SELECT;


    [Header("Animation scroll")]
    [SerializeField]
    private float ScrollAnimationTemps = 2f;

    [SerializeField]
    private AnimationCurve curveScroll;

    [Header("Animation fade")]
    [SerializeField]
    private float fadeAnimationTemps;

    [SerializeField]
    private AnimationCurve courbeFadeOut;

    
    [Header("Select arrows")]

    [SerializeField] private UIBlock2D leftArrow;
    private float BaseLeftArrowPosX;

    [SerializeField] private UIBlock2D rightArrow;
    private float BaseRightArrowPosX;

    [SerializeField] private AnimationCurve arrowCurve;
    public float tempsAnimationArrow;
    public float targetSizeArrow;
    [SerializeField] private Color arrowColorPressed;


    

    // Coroutines
    Coroutine scrollRoutine;
    private Coroutine arrowMovementLeft;
    private Coroutine arrowMovementRight;


    GameObject arrierePlanActuel;

    public static event Action<Camera> onSongSelectEntered;
    public static event Action<int> onMapSelectedChanged;
    public static event Action onMapEnter;

    private static int _mapSelectionneeIndex;

    bool loadingMap = false;

    [ReadOnly]
    public static int MapSelectionneeIndex
    {
        get { return _mapSelectionneeIndex; }
        set
        {
            if (value < 0 || value >= MapCollection.i.mapsAffichees.Count)
            {
                return;
            }
            _mapSelectionneeIndex = value;
            onMapSelectedChanged?.Invoke(value);
        }
    }

    public void ScrollToIndex(int newIndex)
    {
        if (newIndex == MapCollection.i.mapsAffichees.Count)
            return;
        if (scrollRoutine != null)
            StopCoroutine(scrollRoutine);
        scrollRoutine = StartCoroutine(ScrollRoutine(newIndex * -(unset_size + mapList.UIBlock.AutoLayout.Spacing.Value)));
    }

    private void Awake()
    {
        GameManager._mapSelectionneeIndex = 0;
        _mapSelectionneeIndex = 0;
        onSongSelectEntered?.Invoke(camera);
        toursRestants.Text = StaticStats.equipe.NbTours > 1 ? $"{StaticStats.equipe.NbTours} tours restants" : $"Dernier tour!";
        
        Message m = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.WAITINGROOM_STATE);
        m.AddInt((int)WaitingRoomState.LOGO);
        ClientManager.SendMessage(m);
    }

    private void OnEnable()
    {
        onMapSelectedChanged += ScrollToIndex;
        onMapSelectedChanged += MettreArrierePlanAJour;
        onMapSelectedChanged += ChangerCouleurLumieres;
    }

    private void OnDisable()
    {
        onMapSelectedChanged -= ScrollToIndex;
        onMapSelectedChanged -= MettreArrierePlanAJour;
        onMapSelectedChanged -= ChangerCouleurLumieres;
    }

    private void ChangerCouleurLumieres(int obj)
    {
        Message m = Message.Create(
            MessageSendMode.Reliable,
            ClientToServerCalls.SONG_SELECT_CHANGE_BG
        );
        Color c = GameManager.mapSelectionnee.couleur;
        m.Add(c.r).Add(c.g).Add(c.b);
        ClientManager.SendMessage(m);
    }

    public void EchangerKeyCodes()
    {

        if (StaticStats.equipe.NbTours == Constantes.NOMBRE_TOURS)
        {
            ChangerTouchesP1();
            return;
        }
        
        if ((Constantes.NOMBRE_TOURS - StaticStats.equipe.NbTours) % 2 == 1)
        {
            ChangerTouchesP2();
        } else
        {
            ChangerTouchesP1();
            
        }
        
    }

    private void ChangerTouchesP1()
    {
        hotePlayer.Text = $"{StaticStats.equipe.NomPremierJoueur} est l'hôte";
        MOVE_LEFT = Constantes.TOUCHES[0, 0];
        MOVE_RIGHT = Constantes.TOUCHES[0, 3];
        SELECT = Constantes.TOUCHES[0, 1];
        RANDOM_MAP = Constantes.TOUCHES[0, 2];
    }

    private void ChangerTouchesP2()
    {
        hotePlayer.Text = $"{StaticStats.equipe.NomSecondJoueur} est l'hôte";
        MOVE_LEFT = Constantes.TOUCHES[1, 0];
        MOVE_RIGHT = Constantes.TOUCHES[1, 3];
        SELECT = Constantes.TOUCHES[1, 1];
        RANDOM_MAP = Constantes.TOUCHES[1, 2];
    }

    private void MettreArrierePlanAJour(int index)
    {
        StartCoroutine(FadeRoutine(MapCollection.i.mapsAffichees[index].imageArriere));
    }
    
    private IEnumerator MoveArrow(float baseLeftArrowPos, UIBlock arrow, float offset)
    {
        arrow.Position.X.Raw = baseLeftArrowPos;
        arrow.Color = Color.white;
        float tailleInitiale = arrow.Position.X.Raw;
        float timer = 0;
        while (timer < tempsAnimationArrow)
        {
            var lerpVal = 
            arrow.Position.X.Raw = Mathf.Lerp(
                tailleInitiale,
                tailleInitiale + offset,
                arrowCurve.Evaluate(timer / tempsAnimationArrow)
            );
            arrow.Color = Color.Lerp(Color.white, arrowColorPressed, arrowCurve.Evaluate(timer / tempsAnimationArrow));
            yield return null;
            timer += Time.deltaTime;
        }
    }
    
    private void Update()
    {
        if (loadingMap)
            return;

        if (Input.GetKeyDown(MOVE_RIGHT))
        {
            GameManager._mapSelectionneeIndex = MapSelectionneeIndex + 1;
            MapSelectionneeIndex++;
            GameManager.Instance.TutorielEnCours = false;
            if (arrowMovementRight != null)
            {
                StopCoroutine(arrowMovementRight);
            }
            
            arrowMovementRight = StartCoroutine(MoveArrow(BaseRightArrowPosX, rightArrow, targetSizeArrow));
        }
        if (Input.GetKeyDown(MOVE_LEFT))
        {
            GameManager._mapSelectionneeIndex = MapSelectionneeIndex - 1;
            MapSelectionneeIndex--;
            GameManager.Instance.TutorielEnCours = false;
            if (arrowMovementLeft != null)
            {
                StopCoroutine(arrowMovementLeft);
            }
            arrowMovementLeft = StartCoroutine(MoveArrow(BaseLeftArrowPosX, leftArrow, -targetSizeArrow));
        }
        
        if (Input.GetKeyDown(SELECT))
        {

            loadingMap = true;
            onMapEnter?.Invoke();
            SceneManager.LoadScene((int)Scenes.LOADING, LoadSceneMode.Additive);
        }

        if (Input.GetKeyDown(RANDOM_MAP))
        {
            if (MapCollection.i.mapsAffichees.Count <= 1)
                return;

            System.Random rnd = new();
            int mapChoisie = rnd.Next(MapCollection.i.mapsAffichees.Count);
            while (mapChoisie == MapSelectionneeIndex)
            {
                mapChoisie = rnd.Next(MapCollection.i.mapsAffichees.Count);
            }
            GameManager._mapSelectionneeIndex = mapChoisie;
            MapSelectionneeIndex = mapChoisie;
            GameManager.Instance.TutorielEnCours = false;
        }
    }

    private IEnumerator ScrollRoutine(float positionFinale)
    {
        float init = scroller.AutoLayout.Offset;
        float timer = 0;
        while (timer < ScrollAnimationTemps)
        {
            scroller.AutoLayout.Offset =
                Mathf.Lerp(init, positionFinale, curveScroll.Evaluate(timer / ScrollAnimationTemps));
            yield return null;
            timer += Time.deltaTime;
        }
    }

    private IEnumerator FadeRoutine(Texture2D texture) 
    {
        GameObject objetADetruire = arrierePlanActuel;
        UIBlock2D nouveauArrierePlan = PoolManager.i.ClientPools.backgroundPool
            .Spawn(arrierePlansParent)
            .GetComponent<UIBlock2D>();
        nouveauArrierePlan.GetComponent<BlurEffect>().InputTexture = texture;
        Color color = nouveauArrierePlan.Color;
        arrierePlanActuel = nouveauArrierePlan.gameObject;

        float timer = 0;
        while (timer < fadeAnimationTemps)
        {
            timer += Time.deltaTime;
            nouveauArrierePlan.Color = new Color(
                color.r,
                color.g,
                color.b,
                courbeFadeOut.Evaluate(timer / fadeAnimationTemps)
            );
            yield return null;
        }
        
        nouveauArrierePlan.Color = color;

        if (objetADetruire != null)
            LeanPool.Despawn(objetADetruire);
    }

    float unset_size;

    private void Start()
    {
        unset_size = mapList.listItemPrefabs[0].UIBlock.Size.X.Value;
        mapList.UIBlock.Position.X.Raw = (uiRoot.Size.X.Value - MapSelectMapObject.SIZE_SELECTED) / 2;
        mapList.AddDataBinder<MapCollectionMap, MapVisuals>(BindMaps);
        mapList.SetDataSource(MapCollection.i.mapsAffichees);
        MapSelectionneeIndex = 0;
        BaseLeftArrowPosX = leftArrow.Position.X.Raw;
        BaseRightArrowPosX = rightArrow.Position.X.Raw;
        EchangerKeyCodes();
    }



    private void BindMaps(Data.OnBind<MapCollectionMap> evt, MapVisuals visuals, int index)
    {
        visuals.background.InputTexture = evt.UserData.imageCover;
        visuals.mapName.Text = evt.UserData.mapMetadonnees.titre;
        //visuals.obj.setIndex(index);
        visuals.obj.myIndex = index;
        visuals.artistName.Text = evt.UserData.mapMetadonnees.artiste;
        visuals.creatorName.Text = "Mappé par : " + evt.UserData.mapMetadonnees.mappeur;

        DateTime time = new(0);
        time = time.AddSeconds(
            evt.UserData.mapMetadonnees.duree.fin - evt.UserData.mapMetadonnees.duree.debut
        );
        visuals.diff.Text = evt.UserData.mapMetadonnees.difficulte.ToString();
        visuals.time.Text = $"{time:mm:ss}";
        visuals.bpm.Text = evt.UserData.mapMetadonnees.bpm.ToString();
        visuals.ar.Text = evt.UserData.mapMetadonnees.ar.ToString();
    }
}
