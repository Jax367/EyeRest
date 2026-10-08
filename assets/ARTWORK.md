# 艺术层主题插画

## v1.5 新增：Q 版 · 倦怠插兜

`netzach-weary.png` 是新增可选形象，1024 × 1536 RGBA PNG。参考用户提供的倦怠表情，用内置 image_gen 生成：半闭眼、冷淡疲倦的神情，双手插裤兜，双脚落地自然站立，没有武器和战斗动作。原有两个选项保持不变。

采用第一版生成结果（`netzach-weary-generated.png`），通过离线工具只清除格子背景透明度，全部 RGB 颜色值保持不变。已检查侧发与肩膀、右侧头发空隙及双腿之间的真实透明度。完整生成提示词和弃用的去底尝试记录见 `netzach-weary-prompt.txt`。

当前新增资源 SHA-256：`2700B8939D014E92A8C345CAB18ABE189A01A3483808979870778ADC72724D7F`。

## v1.3.2 当前使用素材

默认 Q 版使用 `netzach-user-chibi-transparent.png`，由下述图 1 原文件只清除背景 alpha 得到，保留 1086 × 1448 尺寸和每个像素的 RGB 通道，面部区域 RGBA 逐像素校验不变。没有重绘、调色或缩放。离线处理源码为 `tools/RemoveChibiBackground.cs`，不增加程序运行时开销。

SHA-256：`1292BCD58CFA30195D8F5A73BC13E2A732D588B4EDE632F8E86E263582F942E3`。补除长侧发与角色右肩之间独立封闭的三角背景区域。第二张立绘继续使用下述原图。自检同时验证内嵌资源哈希、各背景空隙的透明度及控件绘制到主题底色的实际结果。

曾尝试内置 image_gen，因结果改变人物细节而弃用，未将生成结果用于程序。提示词核心要求为：只去格子背景，真实透明 alpha，保持人物全部线条、姿势、纹理和颜色，尤其保持现有面部颜色，不重绘、不调色、不缩放。最终采用原图像素的背景透明度处理。

## v1.3 原始素材（保留）

当前直接使用用户本轮提供的两个 PNG 文件，原样复制并逐字节内嵌到 EXE。没有生成、重绘、抠图、缩小文件或重新编码，仅由 GUI 在显示时保持比例缩放。第一张的格子底属于原文件内容，按要求保留。

- `netzach-user-chibi.png`：默认 Q 版，对应图 1。SHA-256：`5B620494C55CFD49849EE1C5741E9858BF6BDB5FDBAD44B0DA6D14AABB900D98`。
- `netzach-user-standing.png`：可选立绘，对应图 2。SHA-256：`9241ECE53FAA22EF0D7870451CCC1680E498361BD2D30C833E551A93C910DC14`。

构建自检会核对 EXE 中两份资源的完整字节哈希。

## v1.2 历史素材（不再内嵌）

`netzach-chibi.png` 是当前程序实际使用的内嵌插画，512 × 768 RGBA PNG，具有真实透明通道。使用内置 image_gen，以用户提供的人物立绘和 Q 版参考图为依据生成，随后通过同一工具移除背景，最后仅做等比例缩小以控制体积。首次显示艺术层主题时才解码。

形象改为接近游戏参考的 Q 版比例、深色手绘线条和平涂颜色，保留绿色发髻、长侧发、长外套、白衬衫、黑领带和圆形胸针，眉眼慵懒且略带微笑。人物不附带书库场景。

### 当前素材生成提示词

Use case: stylized-concept. Create ONE production-ready character sprite for a desktop break reminder. The two provided images are identity, outfit, and ORIGINAL ART STYLE references for Netzach from Library of Ruina. Closely follow the second reference's chibi game-sprite drawing style, but a touch friendlier. Full body adult male character in CHIBI proportions, about 3 heads tall, oversized head and compact narrow body. Keep muted olive-green hair in a loose high bun, long asymmetric side fringe on the left side of the image, sleepy half-lidded eyes with subtle warm expression, tiny gentle closed-mouth smile, pale face. Preserve the reference outfit: long dark olive coat with light edge piping, white button-up shirt, loose narrow black necktie, green circular brooch, black pants and lace-up dark shoes. Relaxed standing pose with one hand loosely in coat pocket and the other hand making a small inviting open-palm gesture. Hand-drawn 2D game sprite: slightly uneven dark ink contours, angular simple shapes, rough pencil-like inner lines, restrained flat cel colors with very light paper/pencil texture ON the character only. NO realistic anatomy, NO painterly cinematic lighting, NO glossy/anime promotional illustration, NO 3D, NO broad grin, NO child, NO elaborate background. Match original reference linework and subdued olive palette rather than the previous generated realistic portrait. A real transparent alpha background: isolate just the full-body character, no library, scenery, ground, cast shadow, decorative particles, border, text, watermark, white or colored backing, or baked checkerboard. Leave narrow clear margin around complete hair bun and feet, centered composition. Output PNG with transparency, vertical approximately 768x1024.

### 透明背景处理提示词

Use case: background-extraction. Edit target is the attached newly generated chibi Netzach sprite. Preserve the character EXACTLY: same linework, face, hair, proportions, colors, texture, pose, costume, hands, shoes and canvas size. CHANGE ONLY BACKGROUND: remove the baked gray-and-white checkerboard completely, including the gaps between arms/coattails/legs and in the hair bun, and export as a PNG with a REAL TRANSPARENT ALPHA CHANNEL. Outside the character must have alpha=0, NOT white, NOT black, NOT a drawn checkerboard transparency pattern. Maintain clean antialiased character edges. Do not redraw, restyle or add any background or shadow. This is a technical background extraction of the supplied character for compositing over arbitrary UI theme colors, not a new illustration. If the output pipeline cannot emit real alpha, use one perfectly uniform flat bright magenta #FF00FF matte instead of any grid, with no variation or cast shadow.

## v1.1 历史素材（不再内嵌）

`netzach-friendly.jpg` 为程序实际使用的内嵌插画，640 × 960 JPEG。通过内置 image_gen 生成，再进行尺寸与 JPEG 压缩优化。只在首次使用艺术层主题时解码，不引入动画或额外运行时。

人物与场景参考来自用户提供的四张《废墟图书馆》图片。角色保留绿色发髻、长侧发、深绿外套、白衬衫与胸针；调整为放松眉眼、轻微微笑、伸手邀请休息的姿态。背景采用深色书架、树根和黄绿色微光。

程序内的四句台词是为休息提醒原创的角色风格文案，不是游戏原文，也不是配音。主题为非官方同人主题。

## 最终生成提示词（内置 image_gen）

Use case: stylized-concept. Asset type: portrait illustration for a lightweight Windows rest-reminder app, a selectable Library of Ruina Floor of Art / Netzach theme. Input images 1 and 2 are environment and palette references only; input images 3 and 4 are character identity and outfit references. Create ONE new high-quality vertical 2:3 illustration, preferably 768x1152. Depict Netzach recognizably as in reference 3: adult male, muted moss-green long hair tied in a loose high bun, asymmetric long side lock, pale skin, slightly sleepy amber/olive eyes, white button-up shirt, loose dark tie, long deep olive coat with pale trim and circular green brooch. Waist-up or upper-thigh composition, centered, face large and legible at small size, hands natural and relaxed (one hand gently open as if inviting a break). Adapt expression to be subtly kinder: relaxed brows, a tiny warm closed-mouth smile and soft eye contact, preserving his languid tired temperament, not cheerful or childish. Style: refined 2D anime game character illustration with confident angular ink lines and softly painterly cel shading, guided by references. Background is a dim antique library of crooked bookcases, twisting tree roots, sparse tiny yellow-green fireflies and muted golden-green light inspired by images 1-2. Keep background subdued and character distinct with warm rim light. Colors dark olive #1B2115, moss #343D24, antique gold and chartreuse #C1D66E. Soft approachable contemplative mood. Entire character hair bun and visible hands comfortably inside frame, no cropping at head. No text, no speech bubble, no UI, no logos, no watermark, no beer/alcohol, no extra people. This is a new illustration using references, not an edit of the screenshot.
