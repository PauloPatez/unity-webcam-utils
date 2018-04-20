using UnityEngine;
using System.Collections;
using System.Reflection;
namespace Utilities {
	#region Class declarations
	public class ColorData{
		public Vector3 hitPoint;
		public Color color;
		public bool didHit;
		public ColorData(Vector3 point, Color col, bool hit){
			hitPoint = point;
			color = col;
			didHit = hit;
		}
	}
	#endregion
	#region WebCam Utils
	public class WebcamUtils {
		public static float GetRatio(WebCamTexture webcam){
			float ratio = ((float)(webcam.width) / (float)(webcam.height));
			return ratio;
		}
		public static void SetRatio(float ratio, GameObject panel){
			Vector3 scale = panel.transform.localScale;
			scale.x = ratio;
			panel.transform.localScale = scale;
		}
		public static void SetCameraSize(Camera camera, Renderer panel){
			camera.orthographicSize = panel.bounds.size.y / 2;
		}
		public static void SetOrientation(WebCamTexture tex, GameObject panel){
			panel.transform.eulerAngles = new Vector3(-90,0,0);
			panel.transform.Rotate(new Vector3(0, -tex.videoRotationAngle,0));
			float scaleY = tex.videoVerticallyMirrored ? -1.0f : 1.0f;
			Vector3 scale = panel.transform.localScale;
			scale.y *= scaleY;
			panel.transform.localScale = scale;
		}
		public static string GetFrontCamName(){
			var devices = WebCamTexture.devices;
			string name = null;
			for( var i = 0 ; i < devices.Length ; i++ ) {
				if (devices[i].isFrontFacing) {
					name = devices[i].name;
				}
			}
			return name;
		}
		public static string GetBackCamName(){
			var devices = WebCamTexture.devices;
			string name = null;
			for( var i = 0 ; i < devices.Length ; i++ ) {
				if (!devices[i].isFrontFacing) {
					name = devices[i].name;
				}
			}
			return name;
		}
	}
	#endregion
	#region Image Utils
	public class ImageUtils {
		public static Vector2 GetAspectRatio(Vector2 xy){
			float f = xy.x / xy.y;
			int i = 0;
			while(true){
				i++;
				if(System.Math.Round(f * i, 2) == Mathf.RoundToInt(f * i))
					break;
			}
			return new Vector2((float)System.Math.Round(f * i, 2), i);
		}
		public static float GetRatio(Texture2D tex){
			float ratio = (float)tex.width / (float)tex.height;
			return ratio;
		}
		public static void SetRatio(float ratio, GameObject panel){
			Vector3 scale = panel.transform.localScale;
			scale.x = scale.y;
			scale.x *= ratio;
			panel.transform.localScale = scale;
		}
		public static Texture2D CapturePhoto(GameObject panel, Camera cam){
			Texture2D tex = panel.GetComponent<Renderer> ().material.mainTexture as Texture2D;
			float multiplier;
			if (tex.width > tex.height) {
				multiplier = (float)tex.width / (float)Screen.width;
			} else {
				multiplier = (float)tex.height / (float)Screen.height;
			}
			int resWidth = (int)((float)Screen.width * (float)multiplier);
			int resHeight = (int)((float)Screen.height * (float)multiplier);
			RenderTexture rt = new RenderTexture (resWidth, resHeight, 24);
			cam.GetComponent<Camera> ().targetTexture = rt;
			Texture2D screenShot;
			if (tex.width > tex.height) {
				screenShot = new Texture2D (resWidth, tex.height, TextureFormat.RGB24, false);
			} else {
				screenShot = new Texture2D (tex.width, resHeight, TextureFormat.RGB24, false);
			}
			cam.GetComponent<Camera> ().Render ();
			rt.filterMode = FilterMode.Point;
			RenderTexture.active = rt;
			Rect rect;
			if (tex.width > tex.height) {
				rect = new Rect(0,(resHeight / 2) - (tex.height / 2),resWidth, tex.height);
			}else{
				rect = new Rect((resWidth / 2) - (tex.width / 2),0,tex.width, resHeight);
			}
			screenShot.filterMode = FilterMode.Point;
			screenShot.ReadPixels (rect, 0, 0);
			screenShot.Apply ();
			Texture2D photo = screenShot as Texture2D;
			cam.GetComponent<Camera> ().targetTexture = null;
			RenderTexture.active = null;
			Object.Destroy (rt);
			return photo;
		}
		public static ColorData GetPixelFromPosition(Camera camera, Vector3 position, GameObject obj, int layerMask){
			RaycastHit hit;
			if (Physics.Raycast (camera.ScreenPointToRay (position).origin, camera.ScreenPointToRay (position).direction, out hit, Mathf.Infinity, 1 << layerMask)) {
				Renderer renderer = hit.collider.gameObject.GetComponent<Renderer> ();
				Texture2D tex = renderer.material.mainTexture as Texture2D;				
				Vector2 pixelUV = hit.textureCoord;
				return new ColorData (hit.point, tex.GetPixelBilinear (pixelUV.x, pixelUV.y), true);
			} else {
				return new ColorData (new Vector2(0,0), new Color(0,0,0), false);
			}
		}
		public static void ScaleToScreen(GameObject obj, Camera camera, float distance){
			Vector3 worldPoint = camera.ScreenToWorldPoint ( new Vector3(0,0,distance) );
			Vector3 scale = obj.transform.lossyScale;
			float xRatio = (-worldPoint.x * 2) / scale.x;
			float yRatio = (-worldPoint.y * 2) / scale.y;
			if (xRatio >= yRatio) {
				obj.transform.localScale *= yRatio;
			} else {
				obj.transform.localScale *= xRatio;
			}
		}

		public static void ScaleToUIRect(GameObject obj, GameObject uiObj){
			Vector3[] corners = new Vector3[4];
			uiObj.GetComponent<RectTransform> ().GetWorldCorners (corners);
			Vector3 scale = obj.transform.lossyScale;
			float xSize = corners[2].x - corners[0].x;
			float ySize = corners[1].y - corners[0].y;
			float xRatio = xSize / scale.x;
			float yRatio = ySize / scale.y;
			if (xRatio >= yRatio) {
				obj.transform.localScale *= yRatio;
			} else {
				obj.transform.localScale *= xRatio;
			}
		}
		public static void ScaleUIRectToSprite(GameObject obj){
			UnityEngine.UI.Image image = obj.GetComponent<UnityEngine.UI.Image> ();
			Vector2 size = new Vector2 (image.sprite.texture.width, image.sprite.texture.height);
			RectTransform rect = obj.GetComponent<RectTransform> ();
			float spriteRatio = size.x / size.y;
			float rectRatio = rect.sizeDelta.x / rect.sizeDelta.y;
			if (spriteRatio > rectRatio) {
				Vector2 rectSize = rect.sizeDelta;
				rectSize.y = rect.sizeDelta.x / spriteRatio;
				rect.sizeDelta = rectSize;
			} else {
				Vector2 rectSize = rect.sizeDelta;
				rectSize.x = rect.sizeDelta.y / (size.y / size.x);
				rect.sizeDelta = rectSize;
			}
		}
		public static void CenterToUIRect(GameObject obj, GameObject uiObj){
			Vector3[] corners = new Vector3[4];
			uiObj.GetComponent<RectTransform> ().GetWorldCorners (corners);
			float xPos = Mathf.Lerp(corners[0].x, corners[2].x, 0.5f);
			float yPos = Mathf.Lerp(corners[0].y, corners[1].y, 0.5f);
			obj.transform.position = new Vector3 (xPos, yPos, corners[0].z);
		}
	}
	#endregion
	#region Color Utils
	public class ColorUtils {
		public static Color HSVToRGB(float h, float s, float v) {
			float r = 0;
			float g = 0;
			float b = 0;
			float i;
			float f;
			float p;
			float q;
			float t;
			i = Mathf.Floor(h * 6);
			f = h * 6 - i;
			p = v * (1 - s);
			q = v * (1 - f * s);
			t = v * (1 - (1 - f) * s);
			switch ((int)i % 6) {
				case 0: r = v; g = t; b = p; break;
				case 1: r = q; g = v; b = p; break;
				case 2: r = p; g = v; b = t; break;
				case 3: r = p; g = q; b = v; break;
				case 4: r = t; g = p; b = v; break;
				case 5: r = v; g = p; b = q; break;
			}
			Color color = new Color (r, g, b);
			return color;
		}
		public static Vector3 RGBToHSV(Color rgb)
		{
			float R = rgb.r;
			if (R<0){
				R=0;}
			else if (R>1){
				R=1;}
			float G = rgb.g;
			if (G<0){
				G=0;}
			else if (G>1){
				G=1;}
			float B = rgb.b;
			if (B<0){
				B=0;}
			else if (B>1){
				B=1;}
			float H;
			float S;
			float V;
			var min = Mathf.Min(R,G,B);
			var max = Mathf.Max(R,G,B);
			float achromatic;
			float delta;
			Vector3 hsv;
			
			if (R==G && G==B){
				achromatic = 1;
			}else{
				achromatic = 0;
			}
			V = max;
			delta = max - min;
			if( max != 0 ){
				S = delta / max;
			}else {
				S = 0;                  
				V = 0;                  
				H = 0;
				hsv.x = H;
				hsv.y = S;
				hsv.z = V;
				return hsv;
			}
			if (achromatic == 1){
				H = 0;
			}else if( R == max ){
				H = 0 + ( G - B ) / delta;
			}else if( G == max ){
				H = 2 + ( B - R ) / delta;
			}else{
				H = 4 + ( R - G ) / delta;
			}
			hsv.x = H;
			hsv.y = S;
			hsv.z = V;
			return hsv;
		}
	}
	#endregion
	#region General Utils
	public class GeneralUtils{
		#if UNITY_ANDROID
		public static void CopyComponents(GameObject from,
		                                  GameObject to,
		                                  int skipAmount,
		                                  int keepAmount){
			
			Component[] components = from.GetComponents(typeof(Component));
			Component[] oldComponents = to.GetComponents(typeof(Component));
			for (int i = keepAmount + 1; i < oldComponents.Length; i++){
				Object.Destroy(oldComponents[i]);
			}
			for (int i = skipAmount + 1; i < components.Length; i++){
				Component new_component = to.AddComponent(components[i].GetType());
				foreach (FieldInfo f in components[i].GetType().GetFields())
				{
					f.SetValue(new_component, f.GetValue(components[i]));
				}
			}
		}
		#endif
	}
	#endregion
}