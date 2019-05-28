# Face-Person-Identify

<img src="image.png" />
<span>
You must set the Face resource key from the portal.azure.com to the app.config in the value tag of the Key setting.
<br/>
  <userSettings>
    <FaceIdentify.Properties.Settings>
      <setting name="Key" serializeAs="String">
        <value></value>
      </setting>
    </FaceIdentify.Properties.Settings>
</userSettings>
The endpoint is set to westus, so the key should match.
See https://docs.microsoft.com/en-us/azure/cognitive-services/cognitive-services-apis-create-account if you don't have a Face API account yet.
 </span>
