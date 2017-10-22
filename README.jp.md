# LobotomyCorporation_ExtraEditModとは
LobotomyCorporationのゲーム中にLobを追加したり、
アブノーマリティの出現順番を任意の物に設定できるmodとなります。

当初は、翻訳した内容を確認しやすくする為に作りました。

***

### ExtraEditModの配布先
このリポジトリはソースコードのみを公開しており、ExtraEditModの成果物は以下のアップローダーにあります。
また、配布する際、Assembly-CSharp.dllも配布していますが、権利元のProject Moonから許可を頂いて配布しています。

https://ux.getuploader.com/Lobotomy_Corporation_Extra_Edit_Mod/

***
### 準備する物
* Microsoft Visual Studio （UnityをインストールするときについているものでOK）
* Illasm  
* ilasm  

***

### ExtraEditMod作成の手順
1. Assembly-CSharp.dllを中間コード（.il）に変換する
2. ILModIWriterを使用して中間コードを書き直し、ExtraEditModを使用できるようにする
3. 書き換えられた中間コードを再コンパイルし、Assembly-CSharp.dllを生成する
4. ExtraEditModのdllを作成する
5. 作成したAssembly-CSharp.dllで現在のDLLを上書きし、同じフォルダにExtraEditModのdllを追加する
6. 起動して楽しむ

***

### 1,Assembly-CSharp.dllを中間コード（.il）に変換する
Illasmは中間コードを出力するために使用されます。出力ファイル名はAssembly-CSharp.ilです。
Illasmはマイクロソフトのソフトウェアです。

***

### 2.ILModIWriterを使用して中間コードを書き直し、ExtraEditModを使用できるようにする
ILModIWriterから中間コードの3か所を書き換えています
* 自作のmod Dllのバージョンを追加
* ゲーム中のヒエラルキーにExtraEditModを生成するため、タイトルシーンのAwakeの箇所に独自のmod initを追加
* CreatureGenerateModel :: SetCreatureをExtraEditMod側で動作させる様に書き換え

詳細はソースコードのILModWriter.csをご確認ください。
***

### 3. 書き換えられた中間コードを再コンパイルし、Assembly-CSharp.dllを生成する
ilasmを使って再コンパイルする
ilasmはマイクロソフトのソフトウェアです。
.Net Frameworkに添付されていたと思います

***
### 4. ExtraEditModのdllを作成する
作成する前に、Assembly-CSharp.dllの参照をVisual Studioに追加します。
ExtraEditModの各機能に対するクラスファイルは以下の通りです

<dl>
    <dt>ExtraEditMod.cs</dt>
      <dd>　このmodの機能全般を管理するクラスです。このクラスから各機能を実行しています。</dd>
    <dt>OrderCreature.cs</dt>
    <dd>　アブノーマリティの出現順番の機能全般を管理するクラスです</dd>
    <dt>AddLob.cs</dt>
    <dd>　Lobポイントを追加する機能を管理するクラスです</dd>
    <dt>EditSetting.cs</dt>
    <dd>　現在のmodの設定をセーブ・ロードを管理するクラスです</dd>
    <dt>EditSettingData.cs</dt>
    <dd>　modの設定をjson形式で保存するためのクラスです</dd>
</dl>

***
### 5. 作成したAssembly-CSharp.dllで現在のDLLを上書きし、同じフォルダにExtraEditModのdllを追加する
Assembly-CSharp.dll　を上書きする前にバックアップを取るといいでしょう。　

***

### 6. 起動して楽しむ
以上がmod作成手順となります。

***
