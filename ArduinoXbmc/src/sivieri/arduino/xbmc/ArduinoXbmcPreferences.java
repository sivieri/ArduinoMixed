package sivieri.arduino.xbmc;

import android.os.Bundle;
import android.preference.PreferenceActivity;

public class ArduinoXbmcPreferences extends PreferenceActivity {

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		addPreferencesFromResource(R.xml.preferences);
	}
	
}
