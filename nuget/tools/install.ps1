param($installPath, $toolsPath, $package, $project)

$configFile = $project.ProjectItems.Item("web.config")
$configFileFullPath = $configFile.Properties.Item("FullPath").Value
$fileContent =  Get-Content $configFileFullPath

$logIdTemplate = "ELMAH_IO_LOG_ID";
$apiKeyTemplate = "ELMAH_IO_API_KEY"
$newInstall = $fileContent | Select-String $logIdTemplate -Quiet;

if ($newInstall) {
	[void] [System.Reflection.Assembly]::LoadWithPartialName("System.Drawing") 
	[void] [System.Reflection.Assembly]::LoadWithPartialName("System.Windows.Forms") 

	# Show install dialog
	$objForm = New-Object System.Windows.Forms.Form 
	$objForm.Text = "Input Log ID"
	$objForm.Size = New-Object System.Drawing.Size(300,230) 
	$objForm.StartPosition = "CenterScreen"
	$objForm.FormBorderStyle = "FixedDialog"

	$objForm.KeyPreview = $True
	$objForm.Add_KeyDown({if ($_.KeyCode -eq "Enter") 
	    {$script:x=$objTextBox.Text;$script:y=$objTextBox2.Text;$objForm.Close()}})
	$objForm.Add_KeyDown({if ($_.KeyCode -eq "Escape") 
	    {$script:x=$null;y=$null;$objForm.Close()}})

	$OKButton = New-Object System.Windows.Forms.Button
	$OKButton.Location = New-Object System.Drawing.Size(75,150)
	$OKButton.Size = New-Object System.Drawing.Size(75,23)
	$OKButton.Text = "OK"
	$OKButton.Add_Click({$script:x=$objTextBox.Text;$script:y=$objTextBox2.Text;$objForm.Close()})
	$objForm.Controls.Add($OKButton)

	$CancelButton = New-Object System.Windows.Forms.Button
	$CancelButton.Location = New-Object System.Drawing.Size(150,150)
	$CancelButton.Size = New-Object System.Drawing.Size(75,23)
	$CancelButton.Text = "Cancel"
	$CancelButton.Add_Click({$objForm.Close()})
	$objForm.Controls.Add($CancelButton)

	$objLabel = New-Object System.Windows.Forms.Label
	$objLabel.Location = New-Object System.Drawing.Size(10,20) 
	$objLabel.Size = New-Object System.Drawing.Size(280,20) 
	$objLabel.Text = "elmah.io API Key:"
	$objForm.Controls.Add($objLabel) 

	$objTextBox = New-Object System.Windows.Forms.TextBox 
	$objTextBox.Location = New-Object System.Drawing.Size(10,40) 
	$objTextBox.Size = New-Object System.Drawing.Size(260,20) 
	$objForm.Controls.Add($objTextBox) 

	$objLabel2 = New-Object System.Windows.Forms.Label
	$objLabel2.Location = New-Object System.Drawing.Size(10,70) 
	$objLabel2.Size = New-Object System.Drawing.Size(280,20) 
	$objLabel2.Text = "elmah.io Log ID:"
	$objForm.Controls.Add($objLabel2) 

	$objTextBox2 = New-Object System.Windows.Forms.TextBox 
	$objTextBox2.Location = New-Object System.Drawing.Size(10,90) 
	$objTextBox2.Size = New-Object System.Drawing.Size(260,20) 
	$objForm.Controls.Add($objTextBox2) 

	$objForm.Topmost = $True

	$objForm.Add_Shown({$objForm.Activate()})
	[void] $objForm.ShowDialog()

	# Mark log as installed
	try {
		Invoke-WebRequest -Uri https://elmah.io/api/install?logid=$y -Method POST
	}
	catch {}

	# Update web.config
	$fileContent =  $fileContent | Foreach-Object {
		$_ -replace $apikeyTemplate, $x `
		   -replace $logIdTemplate, $y
	}
	Set-Content -Value $fileContent -Path $configFileFullPath
}