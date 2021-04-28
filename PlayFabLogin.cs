using System;
using System.Collections.Generic;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Net.Mail;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Threading.Tasks;


public class PlayFabLogin : MonoBehaviour
{
    //Iniciamos variables
    private string userEmail;
    private string userPassword;
    private string username;
    public GameObject loginPanel;
    public Text messageText;
    private GameObject playerObj = null;
    public GameObject login;
    public GameObject menu;
    private string modK;
    private string question;
    public MPManager mp;

    public static string sceneL;
    public static float posYF = 0.0f;
    public static float posXF = 0.0f;
    public static bool q1 = false;
    public static string SessionTicket = "";
    public static string EntityId="";
    public static bool sceneBool= false;

    private String[] qDataKeys;
    private String[] qDataValues;

    protected String emoderador="soporte@Amelia.com";
    protected String pmoderador="AmeliaWaifu87";

    public String[,] questions;

    private int lastQ = -1;



    public Vector2 newPos;

    public void borrarDespues()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = emoderador,
            Password = pmoderador
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, 
            OnLoginSuccess
            , PlayFabError =>
            {
                Debug.Log("Error");
                });
    }


    //al momento de iniciar ejecuta lo siguiente
    public void Start()
    {
        sceneBool = false;
        //Conecta con el plugin de PlayFab
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
        {
            PlayFabSettings.TitleId = "998FF";
        }
        if (playerObj == null)
            playerObj = GameObject.FindGameObjectWithTag("Player");

    }
    //Se guardan las preferencias del jugador en caso de que el ingreso sea exitoso
    private void OnLoginSuccess(LoginResult result)
    {

        Debug.Log("Congratulations, you made succesfully logged in!");
        
        loginPanel.SetActive(false);
        menu.SetActive(true);

        GetPosition();

    }
    //Se guardan las preferencias del jugador en caso de que el registro sea exitoso
    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
    	SessionTicket= result.SessionTicket;
    	EntityId =result.EntityToken.Entity.Id;

        Debug.Log("Congratulations, you succesfully registered and logged in!");
        loginPanel.SetActive(false);
        menu.SetActive(true);

    }
    //En caso de que no haya ninguna cuenta con estos datos la Registra
    private void OnLoginFailure(PlayFabError error)
    {

        var registerRequest = new RegisterPlayFabUserRequest
        {
            Email = userEmail,
            Password = userPassword,
            Username = username
        } ;
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, OnRegisterFailure);
    }
    //Si no se puede registrar es por que ya existe una cuenta y los datos no concuerdan así que arroja
    //un mensaje de error
    private void OnRegisterFailure(PlayFabError error)
    {
        messageText.text = "Username and password don't match";
        Debug.LogError(error.GenerateErrorReport());
    }

    //Obtiene el Email
    public void GetUserEmail(string emailIn)
    {
        userEmail = emailIn;
    }
    //Obtiene el password
    public void GetUserPassword(string passwordIn)
    {
        userPassword = passwordIn;
    }
    //Obtiene el nombre de usuario
    public void GetUsername(string usernameIn)
    {
        username = usernameIn;
    }

    public void GetQuestion(string questionIn)
    {
        question = questionIn;
    }
    public String SetMod()
    {

        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            if (result.Data != null && result.Data.ContainsKey("editorKey"))
            {
                Debug.Log("Hay key de moderador: " + result.Data["editorKey"].Value);
                modK = result.Data["editorKey"].Value;

            }
        }
            , OnError);

        return modK;
    }

    //Se ejecuta al momento de presionar el botón de Login o Register
    public void OnClickLogin()
    {
        //En caso de que la longitud de la contraseña sea de menos de 6 caracteres arroja un mensaje
        //indicándolo y termina la ejecución
        if (userPassword.Length < 6)
        {
            messageText.text = "Password must have at least 6 characters";
            Debug.Log("Password " +userPassword);
            Debug.Log("Username " +username);
            Debug.Log("Email" + userEmail);

            return;
        }
        //Intenta iniciar sesión con el email y contraseña, en caso de que no pueda iniciar sesión
        //este se dirige a intentar crear una nueva cuenta con esos datos
        var request = new LoginWithEmailAddressRequest
        {
            Email = userEmail,
            Password = userPassword
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }
    void OnError(PlayFabError error)
    {
        messageText.text = "Error!";
        Debug.Log(error.GenerateErrorReport());
    }
    void OnRecoverySuccess(SendAccountRecoveryEmailResult succ)
    {
        messageText.text = "Reestablecimiento de contraseña enviado exitosamente";
    }
    public void GetPosition()
    {

        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnError);

    }

    public void checarModerador()
    {
        

    }

    void OnDataRecieved(GetUserDataResult result)
    {

        Debug.Log("Recieved user position");
        if (result.Data != null && result.Data.ContainsKey("PosX") && result.Data.ContainsKey("PosY") && result.Data.ContainsKey("Scene"))
        {
            
            posXF = float.Parse(result.Data["PosX"].Value);
            posYF = float.Parse(result.Data["PosY"].Value);

            if (result.Data.ContainsKey("Quest1"))
            {
                q1 = bool.Parse(result.Data["Quest1"].Value);
                WinScriots.Pasado1 = q1;
            }

            sceneL = result.Data["Scene"].Value;

        }
        else
        {
            Debug.Log("NoData");
        }
    }

    public void SavePosition()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {"PosX", playerObj.transform.position.x.ToString()},
                {"PosY", playerObj.transform.position.y.ToString()},
                {"Scene", SceneManager.GetActiveScene().name},
                {"Quest1", WinScriots.Pasado1.ToString().ToLower()}
            }
        };
        PlayFabClientAPI.UpdateUserData(request, OnDataSend, OnError);
    }
    public void OnDataSend(UpdateUserDataResult result)
    {
        Debug.Log("DataSent successful");
    }

    public void SendAccountRecoveryEmail()
    {

        var request = new SendAccountRecoveryEmailRequest
        {
            TitleId = "998FF",
            Email = userEmail
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnRecoverySuccess, OnError);
    }

    public void GetQuestionsData()
    {

        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            PlayFabId = "DAD29652B862B704",
            Keys = null
        }, 
        OnQuestionsDataReceived, 
        OnError
        );

    }

    IEnumerator SendingQuestions ()
    {
        String tempQ;

        yield return new WaitForSeconds(1f);

        Debug.Log("Datos de las preguntas");
        GetQuestionsData();

        yield return new WaitForSeconds(2f);

        Debug.Log("LastQF " + lastQ);
        String tempID = (lastQ + 1).ToString();

        for (int i = 0; i < 3; i++)
        {
            if (tempID.Length < 3)
            {
                tempID = "0" + tempID;
            }
        }

        tempID = "q" + tempID;
        tempQ = tempID + question;

        Debug.Log("Subiendo pregunta" + tempID + question);
        var request = new UpdateUserDataRequest
        {

            Data = new Dictionary<string, string>
            {
                {tempID, tempQ}
            },
            Permission = UserDataPermission.Public

        };
        PlayFabClientAPI.UpdateUserData(request, OnDataSend, OnError);

        yield return new WaitForSeconds(2f);

        PlayFabClientAPI.ForgetAllCredentials();

        Debug.Log("Regresando a  usuario");

        var logUser = new LoginWithEmailAddressRequest
        {
            Email = userEmail,
            Password = userPassword
        };

        PlayFabClientAPI.LoginWithEmailAddress(logUser, OnLoginSuccess, OnLoginFailure);
    }



    private void OnQuestionsDataReceived(GetUserDataResult result)
    {
        String tempString = "";
        int counter = 0;
        int tempInt = 0;
        bool tempBool;
        char type = '0';
        string tempQuestion= "";
        int size = 1;


        if (result.Data != null)
        {
            
            qDataValues = result.Data.Values.Select(x => x.Value).ToArray();
            
            for (int i = 0; i < qDataValues.Length; i++)
            {
                tempBool = false;

                foreach (char c in qDataValues[i])
                {
                    
                    if (c == 'q' && tempBool == false)
                    {
                        size = size + 1;
                        tempBool = true;
                    }

                }
            }

            questions = new String[size, 2];
            

            for (int i = 0; i < qDataValues.Length; i++)
            {
                tempBool = false;
                counter = 0;
                type = '0';
                tempQuestion = "";
                tempString = "";

                foreach (char c in qDataValues[i])
                {
                    if(tempBool == false)
                    {
                        if (c == 'q' || c == 'r')
                        {
                            type = c;
                            tempBool = true;
                        }
                    } 
                    else
                    {
                        if (tempBool == true)
                        {
                            if (Char.IsNumber(c) && counter < 3)
                            {
                                tempString = tempString + c;
                                counter = counter + 1;
                            }
                            else
                            {
                                tempQuestion = tempQuestion + c;
                            }
                        }
                    }
                }

                if (counter == 3)
                {
                    tempInt = Convert.ToInt32(tempString);

                    if (type == 'q')
                    {
                        questions[tempInt, 0] = tempQuestion;
                    }
                    if (type == 'r')
                    {
                        questions[tempInt, 1] = tempQuestion;
                    }

                }

                if (lastQ <= tempInt)
                {
                    lastQ = tempInt;
                    
                }


            }
           
        }

    }



    private void OnModLoginSuccess(LoginResult result)
    {
        Debug.Log("Listo para recibir información");
    }

    public void SetQuestionsData()
    {
        
        PlayFabClientAPI.ForgetAllCredentials();

        Debug.Log("Iniciando moderador");

        var logMod = new LoginWithEmailAddressRequest
        {
            Email = emoderador,
            Password = pmoderador
        };
        PlayFabClientAPI.LoginWithEmailAddress(logMod, OnModLoginSuccess, OnLoginFailure);

        StartCoroutine(SendingQuestions());

    }


}

    