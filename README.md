# Face-Person-Identify

<img src="image.png" />
<br/>
<span>
You must set the Face resource key from the portal.azure.com to the app.config in the value tag of the Key setting.
<br/>
  &#x3C;userSettings&#x3E;<br/>
    &#x3C;Face.Properties.Settings&#x3E;<br/>
      &#x3C;setting name=&#x22;Key&#x22; serializeAs=&#x22;String&#x22;&#x3E;<br/>
        &#x3C;value&#x3E;&#x3C;/value&#x3E;<br/>
      &#x3C;/setting&#x3E;<br/>
    &#x3C;/Face.Properties.Settings&#x3E;<br/>
&#x3C;/userSettings&#x3E;<br/>
The current endpoint is set to westus, so the key should match this region.<br/>
You can change the endpoint here:<br/>
      public partial class MainWindow : Window<br/>
    {<br/>
        private const string endPoint = @"https://westus.api.cognitive.microsoft.com/";<br/>
See https://docs.microsoft.com/en-us/azure/cognitive-services/cognitive-services-apis-create-account if you don't have a Face API account yet.<br/>
 </span>
