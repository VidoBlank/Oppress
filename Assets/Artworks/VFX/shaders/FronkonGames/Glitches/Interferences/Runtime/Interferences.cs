////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Martin Bustos @FronkonGames <fronkongames@gmail.com>. All rights reserved.
//
// THIS FILE CAN NOT BE HOSTED IN PUBLIC REPOSITORIES.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine.Rendering.Universal;

namespace FronkonGames.Glitches.Interferences
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary>
  /// 
  /// 🕹️ Online documentation: https://fronkongames.github.io/store/glitches/
  /// 📄 Online demo:          https://fronkongames.github.io/demos-glitches/interferences/
  /// ❤️ More assets:          https://assetstore.unity.com/publishers/62716
  /// 
  /// 💡 Any questions or suggestions? Do not hesitate to send me an email to fronkongames@gmail.com
  /// 💡 Do you want to report an error? Please, send the log file along with the mail.
  /// 
  /// ❤️ Write a review if you found this asset useful, thanks! ❤️
  /// 
  /// </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  [DisallowMultipleRendererFeature(Constants.Asset.Name)]
  public sealed partial class Interferences : ScriptableRendererFeature
  {
    // MUST be named "settings" (lowercase) to be shown in the Render Features inspector.
    public Settings settings = new();

    private RenderPass renderPass;

    /// <summary> Initializes this feature's resources. </summary>
    public override void Create() => renderPass = new RenderPass(settings);

    /// <summary> Injects one or multiple ScriptableRenderPass in the renderer. Called every frame once per camera. </summary>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
      renderPass.renderPassEvent = settings.whenToInsert;

      renderer.EnqueuePass(renderPass);
    }
  }
}
