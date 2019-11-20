using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UndertaleModLib.Util;
using System.Linq;
using System.Windows.Forms;

const string _configuration_pattern = "bulk_sprite_exporter_pattern";
int progress = 0;
string texFolder = GetFolder(FilePath) + "Export_Textures" + Path.DirectorySeparatorChar;
TextureWorker worker = new TextureWorker();
Directory.CreateDirectory(texFolder);
List<string> input = new List<string>();
string _pattern = Configuration[_configuration_pattern];

if (ShowInputDialog() == System.Windows.Forms.DialogResult.Cancel)
	return;

string[] arrayString = input.ToArray();

var _savePattern = String.Join(";", arrayString);
ScriptMessage(_savePattern);
Configuration[_configuration_pattern] = _savePattern;

UpdateProgress();
await DumpSprites();
worker.Cleanup();
HideProgressBar();
ScriptMessage("Export Complete.\n\nLocation: " + texFolder);

void UpdateProgress() {
    UpdateProgressBar(null, "Sprites", progress++, Data.Sprites.Count);
}

string GetFolder(string path) {
    return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
}


async Task DumpSprites() {
    await Task.Run(() => Parallel.ForEach(Data.Sprites, DumpSprite));
}

void DumpSprite(UndertaleSprite sprite) {
	if(arrayString.Contains(sprite.Name.ToString().Replace("\"", "")))
		for (int i = 0; i < sprite.Textures.Count; i++)
			if (sprite.Textures[i]?.Texture != null)
			{
				Directory.CreateDirectory(texFolder + sprite.Name.Content);
				worker.ExportAsPNG(sprite.Textures[i].Texture, texFolder + sprite.Name.Content + "\\" + i + ".png");
			}
				
    UpdateProgress();
}

private  DialogResult ShowInputDialog()
    {
        System.Drawing.Size size = new System.Drawing.Size(400, 400);
        Form inputBox = new Form();

        inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        inputBox.ClientSize = size;
        inputBox.Text = "Sprite exporter";

		
		System.Windows.Forms.ListView sprites_list = new ListView();
		sprites_list.View = View.Details;
		sprites_list.FullRowSelect = true;
		sprites_list.Columns.Add("Export");
		sprites_list.Columns.Add("Sprite name", -2);
		
		foreach(var x in Data.Sprites)
		{
			var _node_name = x.Name.ToString().Replace("\"", "");
			var _node = new ListViewItem();
			_node.SubItems.Add(_node_name);
			sprites_list.Items.Add(_node);
		}
		sprites_list.CheckBoxes = true;

		sprites_list.Size = new System.Drawing.Size(size.Width - 10, size.Height - 50);
        sprites_list.Location = new System.Drawing.Point(5, 5);
		inputBox.Controls.Add(sprites_list);
		
        Button okButton = new Button();
        okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        okButton.Name = "okButton";
        okButton.Size = new System.Drawing.Size(75, 23);
        okButton.Text = "&OK";
        okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, size.Height - 39);
        inputBox.Controls.Add(okButton);

        Button cancelButton = new Button();
        cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new System.Drawing.Size(75, 23);
        cancelButton.Text = "&Cancel";
        cancelButton.Location = new System.Drawing.Point(size.Width - 80, size.Height - 39);
        inputBox.Controls.Add(cancelButton);

        inputBox.AcceptButton = okButton;
        inputBox.CancelButton = cancelButton; 
        DialogResult result = inputBox.ShowDialog();
		
		for(int i =0; i < sprites_list.Items.Count; i++)
			if(sprites_list.Items[i].Checked)
				input.Add(sprites_list.Items[i].SubItems[1].Text);
			
        return result;
    }