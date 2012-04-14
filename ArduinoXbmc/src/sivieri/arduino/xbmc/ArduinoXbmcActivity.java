package sivieri.arduino.xbmc;

import java.util.ArrayList;
import java.util.List;

import org.apache.http.HttpResponse;
import org.apache.http.NameValuePair;
import org.apache.http.client.HttpClient;
import org.apache.http.client.entity.UrlEncodedFormEntity;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.impl.client.DefaultHttpClient;
import org.apache.http.message.BasicNameValuePair;

import android.app.Activity;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.os.Handler;
import android.preference.PreferenceManager;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.EditText;
import android.widget.Toast;

public class ArduinoXbmcActivity extends Activity {
	private SharedPreferences preferences;
	private Handler handler;
	private String message = "";
	private Runnable showUpdate = new Runnable() {

		@Override
		public void run() {
			Toast.makeText(ArduinoXbmcActivity.this, message, Toast.LENGTH_SHORT).show();
		}
		
	};
	
    /** Called when the activity is first created. */
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.main);
        preferences = PreferenceManager.getDefaultSharedPreferences(this);
        handler = new Handler();
    }
    
    @Override
	public boolean onCreateOptionsMenu(Menu menu) {
		menu.add(Menu.NONE, 0, 0, R.string.settings_menu);
    	
		return super.onCreateOptionsMenu(menu);
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		if (item.getItemId() == 0) {
			startActivity(new Intent(this, ArduinoXbmcPreferences.class));
			return true;
		}
		
		return false;
	}

	public void sendCmd(View view) {
    	final EditText editText = (EditText) findViewById(R.id.cmdText);
    	if (editText.getText().length() == 0) {
    		Toast.makeText(this, getString(R.string.empty_text_error), Toast.LENGTH_SHORT).show();
    	}
    	else {
    		Thread connection = new Thread(new Runnable() {
				@Override
				public void run() {
					HttpClient client = new DefaultHttpClient();
			    	HttpPost request = new HttpPost("http://" + preferences.getString("device_address", getString(R.string.default_device_address)));
			    	try {
						List<NameValuePair> parameters = new ArrayList<NameValuePair>(2);
						parameters.add(new BasicNameValuePair("cmd", editText.getText().toString()));
						parameters.add(new BasicNameValuePair("submit", "Submit"));
						request.setEntity(new UrlEncodedFormEntity(parameters));
						HttpResponse response = client.execute(request);
						int code = response.getStatusLine().getStatusCode();
						if (code < 400) {
							message = getString(R.string.send_successful);
							handler.post(showUpdate);
						}
						else {
							message = getString(R.string.send_unsuccessful);
							handler.post(showUpdate);
						}
					} catch (Exception e) {
						message = getString(R.string.send_unsuccessful);
						handler.post(showUpdate);
					}
				}
			});
    		connection.start();
    	}
    }
}