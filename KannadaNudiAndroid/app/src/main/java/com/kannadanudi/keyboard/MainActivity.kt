package com.kannadanudi.keyboard

import android.content.Context
import android.content.Intent
import android.os.Bundle
import android.provider.Settings
import android.view.View
import android.view.inputmethod.InputMethodManager
import android.widget.Button
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity

class MainActivity : AppCompatActivity() {

    private lateinit var btnEnable: Button
    private lateinit var tvInstructions: TextView
    private lateinit var tvSteps: TextView

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        btnEnable = findViewById(R.id.btnEnable)
        tvInstructions = findViewById(R.id.tvInstructions)
        tvSteps = findViewById(R.id.tvSteps)
    }

    override fun onResume() {
        super.onResume()
        checkKeyboardStatus()
    }

    private fun checkKeyboardStatus() {
        val imm = getSystemService(Context.INPUT_METHOD_SERVICE) as InputMethodManager
        val list = imm.enabledInputMethodList

        // Check if our keyboard package is in the enabled list
        val isEnabled = list.any { it.packageName == packageName }

        // Check if our keyboard is the currently selected default
        val currentId = Settings.Secure.getString(contentResolver, Settings.Secure.DEFAULT_INPUT_METHOD)
        val isSelected = currentId != null && currentId.contains(packageName)

        if (!isEnabled) {
            tvInstructions.text = "To use this keyboard, you must enable it in System Settings."
            tvSteps.visibility = View.VISIBLE
            tvSteps.text = "1. Click the button below.\n2. Toggle 'Kannada Nudi Keyboard' to ON.\n3. Return to this app."
            btnEnable.text = "Enable Keyboard in Settings"
            btnEnable.isEnabled = true
            btnEnable.setOnClickListener {
                startActivity(Intent(Settings.ACTION_INPUT_METHOD_SETTINGS))
            }
        } else if (!isSelected) {
            tvInstructions.text = "Great! Now select Kannada Nudi as your active keyboard."
            tvSteps.visibility = View.VISIBLE
            tvSteps.text = "1. Click the button below.\n2. Select 'Kannada Nudi Keyboard' from the list."
            btnEnable.text = "Switch Input Method"
            btnEnable.isEnabled = true
            btnEnable.setOnClickListener {
                imm.showInputMethodPicker()
            }
        } else {
            tvInstructions.text = "You are all set! The keyboard is ready to use."
            tvSteps.visibility = View.GONE
            btnEnable.text = "Keyboard is Ready"
            btnEnable.isEnabled = false
            btnEnable.setOnClickListener(null)
        }
    }
}
