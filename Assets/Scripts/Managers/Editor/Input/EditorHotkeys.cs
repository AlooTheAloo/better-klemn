using System;
using System.Collections;
using System.Collections.Generic;
using Chroma.Editor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EditorHotkeys : MonoBehaviour
{
   [SerializeField] private GameObject CreatePanel;
   [SerializeField] private GameObject panneauEffets;
   public static Action deleteBtnPressed;
   public static Action<int> swapComboPressed;
   public static Action<int> zoomComboPressed;
   public static Action selectAllNotes;
   public static Action deselectAllNotes;
   public static Action spawnClickNote;
   public static Action spawnHoldNote;

   public static Action undoChange;
   public static Action redoChange;

   public static Action saveChanges;

   private const int minusVariation = -1;
   private const int plusVariation = 1;
   private void Update()
   {

         if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
         {
            if (CreatePanel.activeSelf)
            {
               if (Input.GetKeyDown(KeyCode.Equals))
               {
                  zoomComboPressed?.Invoke(plusVariation);
               }

               if (Input.GetKeyDown(KeyCode.Minus))
               {
                  zoomComboPressed?.Invoke(minusVariation);
               }

               if (Input.GetKeyDown(KeyCode.A))
               {
                  selectAllNotes?.Invoke();
               }

               if (Input.GetKeyDown(KeyCode.N))
               {
                  spawnClickNote?.Invoke();
               }

               if (Input.GetKeyDown(KeyCode.H))
               {
                  spawnHoldNote?.Invoke();
               }
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
               swapComboPressed?.Invoke(1);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
               swapComboPressed?.Invoke(2);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
               swapComboPressed?.Invoke(3);
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
               undoChange.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
               redoChange.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
               saveChanges.Invoke();
            }
         }
         
         
         if (Input.GetKeyDown(KeyCode.Escape) && !panneauEffets.activeSelf)
         {
            deselectAllNotes?.Invoke();
         }
         
         if (Input.GetKeyDown(KeyCode.Delete))
         {
            deleteBtnPressed?.Invoke();
         }

      
   }

}