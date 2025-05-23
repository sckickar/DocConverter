using System;

namespace DocGen.Office;

internal class RTLCharacters
{
	private sbyte[] m_types;

	private sbyte m_textOrder = -1;

	private int m_length;

	private sbyte[] m_result;

	private sbyte[] m_levels;

	private sbyte[] m_rtlCharacterTypes = new sbyte[65536];

	private const sbyte L = 0;

	private const sbyte LRE = 1;

	private const sbyte LRO = 2;

	private const sbyte R = 3;

	private const sbyte AL = 4;

	private const sbyte RLE = 5;

	private const sbyte RLO = 6;

	private const sbyte PDF = 7;

	private const sbyte EN = 8;

	private const sbyte ES = 9;

	private const sbyte ET = 10;

	private const sbyte AN = 11;

	private const sbyte CS = 12;

	private const sbyte NSM = 13;

	private const sbyte BN = 14;

	private const sbyte B = 15;

	private const sbyte S = 16;

	private const sbyte WS = 17;

	private const sbyte ON = 18;

	private readonly char[] CharTypes = new char[1725]
	{
		'\0', '\b', '\u000e', '\t', '\t', '\u0010', '\n', '\n', '\u000f', '\v',
		'\v', '\u0010', '\f', '\f', '\u0011', '\r', '\r', '\u000f', '\u000e', '\u001b',
		'\u000e', '\u001c', '\u001e', '\u000f', '\u001f', '\u001f', '\u0010', ' ', ' ', '\u0011',
		'!', '"', '\u0012', '#', '%', '\n', '&', '*', '\u0012', '+',
		'+', '\t', ',', ',', '\f', '-', '-', '\t', '.', '.',
		'\f', '/', '/', '\f', '0', '9', '\b', ':', ':', '\f',
		';', '@', '\u0012', 'A', 'Z', '\0', '[', '`', '\u0012', 'a',
		'z', '\0', '{', '~', '\u0012', '\u007f', '\u0084', '\u000e', '\u0085', '\u0085',
		'\u000f', '\u0086', '\u009f', '\u000e', '\u00a0', '\u00a0', '\f', '¡', '¡', '\u0012',
		'¢', '¥', '\n', '¦', '©', '\u0012', 'ª', 'ª', '\0', '«',
		'\u00af', '\u0012', '°', '±', '\n', '²', '³', '\b', '\u00b4', '\u00b4',
		'\u0012', 'µ', 'µ', '\0', '¶', '\u00b8', '\u0012', '¹', '¹', '\b',
		'º', 'º', '\0', '»', '¿', '\u0012', 'À', 'Ö', '\0', '×',
		'×', '\u0012', 'Ø', 'ö', '\0', '÷', '÷', '\u0012', 'ø', 'ʸ',
		'\0', 'ʹ', 'ʺ', '\u0012', 'ʻ', 'ˁ', '\0', '\u02c2', 'ˏ', '\u0012',
		'ː', 'ˑ', '\0', '\u02d2', '\u02df', '\u0012', 'ˠ', 'ˤ', '\0', '\u02e5',
		'\u02ed', '\u0012', 'ˮ', 'ˮ', '\0', '\u02ef', '\u02ff', '\u0012', '\u0300', '\u0357',
		'\r', '\u0358', '\u035c', '\0', '\u035d', '\u036f', '\r', 'Ͱ', 'ͳ', '\0',
		'ʹ', '\u0375', '\u0012', 'Ͷ', 'ͽ', '\0', ';', ';', '\u0012', 'Ϳ',
		'\u0383', '\0', '\u0384', '\u0385', '\u0012', 'Ά', 'Ά', '\0', '·', '·',
		'\u0012', 'Έ', 'ϵ', '\0', '϶', '϶', '\u0012', 'Ϸ', '҂', '\0',
		'\u0483', '\u0486', '\r', '\u0487', '\u0487', '\0', '\u0488', '\u0489', '\r', 'Ҋ',
		'։', '\0', '֊', '֊', '\u0012', '\u058b', '\u0590', '\0', '\u0591', '\u05a1',
		'\r', '\u05a2', '\u05a2', '\0', '\u05a3', '\u05b9', '\r', '\u05ba', '\u05ba', '\0',
		'\u05bb', '\u05bd', '\r', '־', '־', '\u0003', '\u05bf', '\u05bf', '\r', '׀',
		'׀', '\u0003', '\u05c1', '\u05c2', '\r', '׃', '׃', '\u0003', '\u05c4', '\u05c4',
		'\r', '\u05c5', '\u05cf', '\0', 'א', 'ת', '\u0003', '\u05eb', 'ׯ', '\0',
		'װ', '״', '\u0003', '\u05f5', '\u05ff', '\0', '\u0600', '\u0603', '\u0004', '\u0604',
		'؋', '\0', '،', '،', '\f', '؍', '؍', '\u0004', '؎', '؏',
		'\u0012', '\u0610', '\u0615', '\r', '\u0616', '\u061a', '\0', '؛', '؛', '\u0004',
		'\u061c', '؞', '\0', '؟', '؟', '\u0004', 'ؠ', 'ؠ', '\0', 'ء',
		'غ', '\u0004', 'ػ', 'ؿ', '\0', 'ـ', 'ي', '\u0004', '\u064b', '\u0658',
		'\r', '\u0659', '\u065f', '\0', '٠', '٩', '\v', '٪', '٪', '\n',
		'٫', '٬', '\v', '٭', 'ٯ', '\u0004', '\u0670', '\u0670', '\r', 'ٱ',
		'ە', '\u0004', '\u06d6', '\u06dc', '\r', '\u06dd', '\u06dd', '\u0004', '۞', '\u06e4',
		'\r', 'ۥ', 'ۦ', '\u0004', '\u06e7', '\u06e8', '\r', '۩', '۩', '\u0012',
		'\u06ea', '\u06ed', '\r', 'ۮ', 'ۯ', '\u0004', '۰', '۹', '\b', 'ۺ',
		'܍', '\u0004', '\u070e', '\u070e', '\0', '\u070f', '\u070f', '\u000e', 'ܐ', 'ܐ',
		'\u0004', '\u0711', '\u0711', '\r', 'ܒ', 'ܯ', '\u0004', '\u0730', '\u074a', '\r',
		'\u074b', '\u074c', '\0', 'ݍ', 'ݏ', '\u0004', 'ݐ', 'ݿ', '\0', 'ހ',
		'ޥ', '\u0004', '\u07a6', '\u07b0', '\r', 'ޱ', 'ޱ', '\u0004', '\u07b2', '\u0900',
		'\0', '\u0901', '\u0902', '\r', '\u0903', '\u093b', '\0', '\u093c', '\u093c', '\r',
		'ऽ', '\u0940', '\0', '\u0941', '\u0948', '\r', '\u0949', '\u094c', '\0', '\u094d',
		'\u094d', '\r', '\u094e', 'ॐ', '\0', '\u0951', '\u0954', '\r', '\u0955', 'ॡ',
		'\0', '\u0962', '\u0963', '\r', '।', 'ঀ', '\0', '\u0981', '\u0981', '\r',
		'\u0982', '\u09bb', '\0', '\u09bc', '\u09bc', '\r', 'ঽ', '\u09c0', '\0', '\u09c1',
		'\u09c4', '\r', '\u09c5', '\u09cc', '\0', '\u09cd', '\u09cd', '\r', 'ৎ', 'ৡ',
		'\0', '\u09e2', '\u09e3', '\r', '\u09e4', 'ৱ', '\0', '৲', '৳', '\n',
		'৴', '\u0a00', '\0', '\u0a01', '\u0a02', '\r', '\u0a03', '\u0a3b', '\0', '\u0a3c',
		'\u0a3c', '\r', '\u0a3d', '\u0a40', '\0', '\u0a41', '\u0a42', '\r', '\u0a43', '\u0a46',
		'\0', '\u0a47', '\u0a48', '\r', '\u0a49', '\u0a4a', '\0', '\u0a4b', '\u0a4d', '\r',
		'\u0a4e', '੯', '\0', '\u0a70', '\u0a71', '\r', 'ੲ', '\u0a80', '\0', '\u0a81',
		'\u0a82', '\r', '\u0a83', '\u0abb', '\0', '\u0abc', '\u0abc', '\r', 'ઽ', '\u0ac0',
		'\0', '\u0ac1', '\u0ac5', '\r', '\u0ac6', '\u0ac6', '\0', '\u0ac7', '\u0ac8', '\r',
		'\u0ac9', '\u0acc', '\0', '\u0acd', '\u0acd', '\r', '\u0ace', 'ૡ', '\0', '\u0ae2',
		'\u0ae3', '\r', '\u0ae4', '૰', '\0', '૱', '૱', '\n', '\u0af2', '\u0b00',
		'\0', '\u0b01', '\u0b01', '\r', '\u0b02', '\u0b3b', '\0', '\u0b3c', '\u0b3c', '\r',
		'ଽ', '\u0b3e', '\0', '\u0b3f', '\u0b3f', '\r', '\u0b40', '\u0b40', '\0', '\u0b41',
		'\u0b43', '\r', '\u0b44', '\u0b4c', '\0', '\u0b4d', '\u0b4d', '\r', '\u0b4e', '\u0b55',
		'\0', '\u0b56', '\u0b56', '\r', '\u0b57', '\u0b81', '\0', '\u0b82', '\u0b82', '\r',
		'ஃ', '\u0bbf', '\0', '\u0bc0', '\u0bc0', '\r', '\u0bc1', '\u0bcc', '\0', '\u0bcd',
		'\u0bcd', '\r', '\u0bce', '௲', '\0', '௳', '௸', '\u0012', '௹', '௹',
		'\n', '௺', '௺', '\u0012', '\u0bfb', 'ఽ', '\0', '\u0c3e', '\u0c40', '\r',
		'\u0c41', '\u0c45', '\0', '\u0c46', '\u0c48', '\r', '\u0c49', '\u0c49', '\0', '\u0c4a',
		'\u0c4d', '\r', '\u0c4e', '\u0c54', '\0', '\u0c55', '\u0c56', '\r', '\u0c57', '\u0cbb',
		'\0', '\u0cbc', '\u0cbc', '\r', 'ಽ', '\u0ccb', '\0', '\u0ccc', '\u0ccd', '\r',
		'\u0cce', '\u0d40', '\0', '\u0d41', '\u0d43', '\r', '\u0d44', '\u0d4c', '\0', '\u0d4d',
		'\u0d4d', '\r', 'ൎ', '\u0dc9', '\0', '\u0dca', '\u0dca', '\r', '\u0dcb', '\u0dd1',
		'\0', '\u0dd2', '\u0dd4', '\r', '\u0dd5', '\u0dd5', '\0', '\u0dd6', '\u0dd6', '\r',
		'\u0dd7', 'ะ', '\0', '\u0e31', '\u0e31', '\r', 'า', 'ำ', '\0', '\u0e34',
		'\u0e3a', '\r', '\u0e3b', '\u0e3e', '\0', '฿', '฿', '\n', 'เ', 'ๆ',
		'\0', '\u0e47', '\u0e4e', '\r', '๏', 'ະ', '\0', '\u0eb1', '\u0eb1', '\r',
		'າ', 'ຳ', '\0', '\u0eb4', '\u0eb9', '\r', '\u0eba', '\u0eba', '\0', '\u0ebb',
		'\u0ebc', '\r', 'ຽ', '\u0ec7', '\0', '\u0ec8', '\u0ecd', '\r', '\u0ece', '༗',
		'\0', '\u0f18', '\u0f19', '\r', '༚', '༴', '\0', '\u0f35', '\u0f35', '\r',
		'༶', '༶', '\0', '\u0f37', '\u0f37', '\r', '༸', '༸', '\0', '\u0f39',
		'\u0f39', '\r', '༺', '༽', '\u0012', '\u0f3e', '\u0f70', '\0', '\u0f71', '\u0f7e',
		'\r', '\u0f7f', '\u0f7f', '\0', '\u0f80', '\u0f84', '\r', '྅', '྅', '\0',
		'\u0f86', '\u0f87', '\r', 'ྈ', '\u0f8f', '\0', '\u0f90', '\u0f97', '\r', '\u0f98',
		'\u0f98', '\0', '\u0f99', '\u0fbc', '\r', '\u0fbd', '࿅', '\0', '\u0fc6', '\u0fc6',
		'\r', '࿇', '\u102c', '\0', '\u102d', '\u1030', '\r', '\u1031', '\u1031', '\0',
		'\u1032', '\u1032', '\r', '\u1033', '\u1035', '\0', '\u1036', '\u1037', '\r', '\u1038',
		'\u1038', '\0', '\u1039', '\u1039', '\r', '\u103a', '\u1057', '\0', '\u1058', '\u1059',
		'\r', 'ၚ', 'ᙿ', '\0', '\u1680', '\u1680', '\u0011', 'ᚁ', 'ᚚ', '\0',
		'᚛', '᚜', '\u0012', '\u169d', 'ᜑ', '\0', '\u1712', '\u1714', '\r', '\u1715',
		'ᜱ', '\0', '\u1732', '\u1734', '\r', '᜵', 'ᝑ', '\0', '\u1752', '\u1753',
		'\r', '\u1754', '\u1771', '\0', '\u1772', '\u1773', '\r', '\u1774', '\u17b6', '\0',
		'\u17b7', '\u17bd', '\r', '\u17be', '\u17c5', '\0', '\u17c6', '\u17c6', '\r', '\u17c7',
		'\u17c8', '\0', '\u17c9', '\u17d3', '\r', '។', '៚', '\0', '៛', '៛',
		'\n', 'ៜ', 'ៜ', '\0', '\u17dd', '\u17dd', '\r', '\u17de', '\u17ef', '\0',
		'៰', '៹', '\u0012', '\u17fa', '\u17ff', '\0', '᠀', '᠊', '\u0012', '\u180b',
		'\u180d', '\r', '\u180e', '\u180e', '\u0011', '\u180f', 'ᢨ', '\0', '\u18a9', '\u18a9',
		'\r', 'ᢪ', '\u191f', '\0', '\u1920', '\u1922', '\r', '\u1923', '\u1926', '\0',
		'\u1927', '\u192b', '\r', '\u192c', '\u1931', '\0', '\u1932', '\u1932', '\r', '\u1933',
		'\u1938', '\0', '\u1939', '\u193b', '\r', '\u193c', '\u193f', '\0', '᥀', '᥀',
		'\u0012', '\u1941', '\u1943', '\0', '᥄', '᥅', '\u0012', '᥆', '᧟', '\0',
		'᧠', '᧿', '\u0012', 'ᨀ', 'ᾼ', '\0', '\u1fbd', '\u1fbd', '\u0012', 'ι',
		'ι', '\0', '\u1fbf', '\u1fc1', '\u0012', 'ῂ', 'ῌ', '\0', '\u1fcd', '\u1fcf',
		'\u0012', 'ῐ', '\u1fdc', '\0', '\u1fdd', '\u1fdf', '\u0012', 'ῠ', 'Ῥ', '\0',
		'\u1fed', '\u1fef', '\u0012', '\u1ff0', 'ῼ', '\0', '\u1ffd', '\u1ffe', '\u0012', '\u1fff',
		'\u1fff', '\0', '\u2000', '\u200a', '\u0011', '\u200b', '\u200d', '\u000e', '\u200e', '\u200e',
		'\0', '\u200f', '\u200f', '\u0003', '‐', '‧', '\u0012', '\u2028', '\u2028', '\u0011',
		'\u2029', '\u2029', '\u000f', '\u202a', '\u202a', '\u0001', '\u202b', '\u202b', '\u0005', '\u202c',
		'\u202c', '\a', '\u202d', '\u202d', '\u0002', '\u202e', '\u202e', '\u0006', '\u202f', '\u202f',
		'\u0011', '‰', '‴', '\n', '‵', '\u2054', '\u0012', '⁕', '⁖', '\0',
		'⁗', '⁗', '\u0012', '⁘', '⁞', '\0', '\u205f', '\u205f', '\u0011', '\u2060',
		'\u2063', '\u000e', '\u2064', '\u2069', '\0', '\u206a', '\u206f', '\u000e', '⁰', '⁰',
		'\b', 'ⁱ', '\u2073', '\0', '⁴', '⁹', '\b', '⁺', '⁻', '\n',
		'⁼', '⁾', '\u0012', 'ⁿ', 'ⁿ', '\0', '₀', '₉', '\b', '₊',
		'₋', '\n', '₌', '₎', '\u0012', '\u208f', '\u209f', '\0', '₠', '₱',
		'\n', '₲', '\u20cf', '\0', '\u20d0', '\u20ea', '\r', '\u20eb', '\u20ff', '\0',
		'℀', '℁', '\u0012', 'ℂ', 'ℂ', '\0', '℃', '℆', '\u0012', 'ℇ',
		'ℇ', '\0', '℈', '℉', '\u0012', 'ℊ', 'ℓ', '\0', '℔', '℔',
		'\u0012', 'ℕ', 'ℕ', '\0', '№', '℘', '\u0012', 'ℙ', 'ℝ', '\0',
		'℞', '℣', '\u0012', 'ℤ', 'ℤ', '\0', '℥', '℥', '\u0012', 'Ω',
		'Ω', '\0', '℧', '℧', '\u0012', 'ℨ', 'ℨ', '\0', '℩', '℩',
		'\u0012', 'K', 'ℭ', '\0', '℮', '℮', '\n', 'ℯ', 'ℱ', '\0',
		'Ⅎ', 'Ⅎ', '\u0012', 'ℳ', 'ℹ', '\0', '℺', '℻', '\u0012', 'ℼ',
		'ℿ', '\0', '⅀', '⅄', '\u0012', 'ⅅ', 'ⅉ', '\0', '⅊', '⅋',
		'\u0012', '⅌', '⅒', '\0', '⅓', '⅟', '\u0012', 'Ⅰ', '\u218f', '\0',
		'←', '∑', '\u0012', '−', '∓', '\n', '∔', '⌵', '\u0012', '⌶',
		'⍺', '\0', '⍻', '⎔', '\u0012', '⎕', '⎕', '\0', '⎖', '⏐',
		'\u0012', '⏑', '⏿', '\0', '␀', '␦', '\u0012', '\u2427', '\u243f', '\0',
		'⑀', '⑊', '\u0012', '\u244b', '\u245f', '\0', '①', '⒛', '\b', '⒜',
		'ⓩ', '\0', '⓪', '⓪', '\b', '⓫', '☗', '\u0012', '☘', '☘',
		'\0', '☙', '♽', '\u0012', '♾', '♿', '\0', '⚀', '⚑', '\u0012',
		'⚒', '⚟', '\0', '⚠', '⚡', '\u0012', '⚢', '✀', '\0', '✁',
		'✄', '\u0012', '✅', '✅', '\0', '✆', '✉', '\u0012', '✊', '✋',
		'\0', '✌', '✧', '\u0012', '✨', '✨', '\0', '✩', '❋', '\u0012',
		'❌', '❌', '\0', '❍', '❍', '\u0012', '❎', '❎', '\0', '❏',
		'❒', '\u0012', '❓', '❕', '\0', '❖', '❖', '\u0012', '❗', '❗',
		'\0', '❘', '❞', '\u0012', '❟', '❠', '\0', '❡', '➔', '\u0012',
		'➕', '➗', '\0', '➘', '➯', '\u0012', '➰', '➰', '\0', '➱',
		'➾', '\u0012', '➿', '⟏', '\0', '⟐', '⟫', '\u0012', '⟬', '⟯',
		'\0', '⟰', '⬍', '\u0012', '⬎', '\u2e7f', '\0', '⺀', '⺙', '\u0012',
		'\u2e9a', '\u2e9a', '\0', '⺛', '⻳', '\u0012', '\u2ef4', '\u2eff', '\0', '⼀',
		'⿕', '\u0012', '\u2fd6', '\u2fef', '\0', '⿰', '⿻', '\u0012', '\u2ffc', '\u2fff',
		'\0', '\u3000', '\u3000', '\u0011', '、', '〄', '\u0012', '々', '〇', '\0',
		'〈', '〠', '\u0012', '〡', '〩', '\0', '\u302a', '\u302f', '\r', '〰',
		'〰', '\u0012', '〱', '〵', '\0', '〶', '〷', '\u0012', '〸', '〼',
		'\0', '〽', '〿', '\u0012', '\u3040', '\u3098', '\0', '\u3099', '\u309a', '\r',
		'\u309b', '\u309c', '\u0012', 'ゝ', 'ゟ', '\0', '゠', '゠', '\u0012', 'ァ',
		'ヺ', '\0', '・', '・', '\u0012', 'ー', '㈜', '\0', '㈝', '㈞',
		'\u0012', '\u321f', '㉏', '\0', '㉐', '㉟', '\u0012', '㉠', '㉻', '\0',
		'㉼', '㉽', '\u0012', '㉾', '㊰', '\0', '㊱', '㊿', '\u0012', '㋀',
		'㋋', '\0', '㋌', '㋏', '\u0012', '㋐', '㍶', '\0', '㍷', '㍺',
		'\u0012', '㍻', '㏝', '\0', '㏞', '㏟', '\u0012', '㏠', '㏾', '\0',
		'㏿', '㏿', '\u0012', '㐀', '䶿', '\0', '䷀', '䷿', '\u0012', '一',
		'\ua48f', '\0', '꒐', '꓆', '\u0012', '\ua4c7', '\ufb1c', '\0', 'יִ', 'יִ',
		'\u0003', '\ufb1e', '\ufb1e', '\r', 'ײַ', 'ﬨ', '\u0003', '﬩', '﬩', '\n',
		'שׁ', 'זּ', '\u0003', '\ufb37', '\ufb37', '\0', 'טּ', 'לּ', '\u0003', '\ufb3d',
		'\ufb3d', '\0', 'מּ', 'מּ', '\u0003', '\ufb3f', '\ufb3f', '\0', 'נּ', 'סּ',
		'\u0003', '\ufb42', '\ufb42', '\0', 'ףּ', 'פּ', '\u0003', '\ufb45', '\ufb45', '\0',
		'צּ', 'ﭏ', '\u0003', 'ﭐ', 'ﮱ', '\u0004', '\ufbb2', '\ufbd2', '\0', 'ﯓ',
		'ﴽ', '\u0004', '﴾', '﴿', '\u0012', '﵀', '﵏', '\0', 'ﵐ', 'ﶏ',
		'\u0004', '\ufd90', '\ufd91', '\0', 'ﶒ', 'ﷇ', '\u0004', '\ufdc8', '\ufdef', '\0',
		'ﷰ', '﷼', '\u0004', '﷽', '﷽', '\u0012', '﷾', '﷿', '\0', '\ufe00',
		'\ufe0f', '\r', '︐', '\ufe1f', '\0', '\ufe20', '\ufe23', '\r', '\ufe24', '\ufe2f',
		'\0', '︰', '\ufe4f', '\u0012', '﹐', '﹐', '\f', '﹑', '﹑', '\u0012',
		'﹒', '﹒', '\f', '\ufe53', '\ufe53', '\0', '﹔', '﹔', '\u0012', '﹕',
		'﹕', '\f', '﹖', '﹞', '\u0012', '﹟', '﹟', '\n', '﹠', '﹡',
		'\u0012', '﹢', '﹣', '\n', '﹤', '﹦', '\u0012', '\ufe67', '\ufe67', '\0',
		'﹨', '﹨', '\u0012', '﹩', '﹪', '\n', '﹫', '﹫', '\u0012', '\ufe6c',
		'\ufe6f', '\0', 'ﹰ', 'ﹴ', '\u0004', '\ufe75', '\ufe75', '\0', 'ﹶ', 'ﻼ',
		'\u0004', '\ufefd', '\ufefe', '\0', '\ufeff', '\ufeff', '\u000e', '\uff00', '\uff00', '\0',
		'！', '＂', '\u0012', '＃', '％', '\n', '＆', '＊', '\u0012', '＋',
		'＋', '\n', '，', '，', '\f', '－', '－', '\n', '．', '．',
		'\f', '／', '／', '\t', '０', '９', '\b', '：', '：', '\f',
		'；', '＠', '\u0012', 'Ａ', 'Ｚ', '\0', '［', '\uff40', '\u0012', 'ａ',
		'ｚ', '\0', '｛', '･', '\u0012', 'ｦ', '\uffdf', '\0', '￠', '￡',
		'\n', '￢', '￤', '\u0012', '￥', '￦', '\n', '\uffe7', '\uffe7', '\0',
		'￨', '￮', '\u0012', '\uffef', '\ufff8', '\0', '\ufff9', '\ufffb', '\u000e', '￼',
		'\ufffd', '\u0012', '\ufffe', '\uffff', '\0'
	};

	internal RTLCharacters()
	{
		int num;
		for (num = 0; num < CharTypes.Length; num++)
		{
			int num2 = CharTypes[num];
			int num3 = CharTypes[++num];
			sbyte b = (sbyte)CharTypes[++num];
			while (num2 <= num3)
			{
				m_rtlCharacterTypes[num2++] = b;
			}
		}
	}

	internal byte[] GetVisualOrder(OtfGlyphInfo[] inputText, bool isRTL)
	{
		m_types = GetCharacterCode(inputText);
		m_textOrder = (isRTL ? ((sbyte)1) : ((sbyte)0));
		DoVisualOrder();
		byte[] array = new byte[m_result.Length];
		for (int i = 0; i < m_levels.Length; i++)
		{
			array[i] = (byte)m_levels[i];
		}
		return array;
	}

	internal byte[] GetVisualOrder(string inputText, bool isRTL)
	{
		m_types = GetCharacterCode(inputText);
		m_textOrder = (isRTL ? ((sbyte)1) : ((sbyte)0));
		DoVisualOrder();
		byte[] array = new byte[m_result.Length];
		for (int i = 0; i < m_levels.Length; i++)
		{
			array[i] = (byte)m_levels[i];
		}
		return array;
	}

	private sbyte[] GetCharacterCode(string text)
	{
		sbyte[] array = new sbyte[text.Length];
		for (int i = 0; i < text.Length; i++)
		{
			array[i] = m_rtlCharacterTypes[(uint)text[i]];
		}
		return array;
	}

	private sbyte[] GetCharacterCode(OtfGlyphInfo[] text)
	{
		sbyte[] array = new sbyte[text.Length];
		for (int i = 0; i < text.Length; i++)
		{
			_ = 162;
			char c = (char)((text[i].CharCode > 0) ? text[i].CharCode : text[i].Characters[0]);
			array[i] = m_rtlCharacterTypes[(uint)c];
		}
		return array;
	}

	private void SetDefaultLevels()
	{
		for (int i = 0; i < m_length; i++)
		{
			m_levels[i] = m_textOrder;
		}
	}

	private void SetLevels()
	{
		SetDefaultLevels();
		for (int i = 0; i < m_length; i++)
		{
			sbyte b = m_levels[i];
			if (((uint)b & 0x80u) != 0)
			{
				b &= 0x7F;
				m_result[i] = (sbyte)((((uint)b & (true ? 1u : 0u)) != 0) ? 3 : 0);
			}
			m_levels[i] = b;
		}
	}

	private void UpdateLevels(int index, sbyte level, int length)
	{
		if ((level & 1) == 0)
		{
			for (int i = index; i < length; i++)
			{
				if (m_result[i] == 3)
				{
					m_levels[i]++;
				}
				else if (m_result[i] != 0)
				{
					m_levels[i] += 2;
				}
			}
			return;
		}
		for (int j = index; j < length; j++)
		{
			if (m_result[j] != 3)
			{
				m_levels[j]++;
			}
		}
	}

	private void DoVisualOrder()
	{
		m_length = m_types.Length;
		m_result = (sbyte[])m_types.Clone();
		m_levels = new sbyte[m_length];
		SetLevels();
		m_length = GetEmbeddedCharactersLength();
		sbyte val = m_textOrder;
		int num = 0;
		while (num < m_length)
		{
			sbyte b = m_levels[num];
			sbyte startType = (sbyte)((((uint)Math.Max(val, b) & (true ? 1u : 0u)) != 0) ? 3 : 0);
			int i;
			for (i = num + 1; i < m_length && m_levels[i] == b; i++)
			{
			}
			sbyte endType = (sbyte)((((uint)Math.Max((i < m_length) ? m_levels[i] : m_textOrder, b) & (true ? 1u : 0u)) != 0) ? 3 : 0);
			CheckNSM(num, i, b, startType, endType);
			UpdateLevels(num, b, i);
			val = b;
			num = i;
		}
		CheckEmbeddedCharacters(m_length);
	}

	private int GetEmbeddedCharactersLength()
	{
		int num = 0;
		for (int i = 0; i < m_length; i++)
		{
			if (m_types[i] != 1 && m_types[i] != 5 && m_types[i] != 2 && m_types[i] != 6 && m_types[i] != 7 && m_types[i] != 14)
			{
				m_result[num] = m_result[i];
				m_levels[num] = m_levels[i];
				num++;
			}
		}
		return num;
	}

	private void CheckEmbeddedCharacters(int length)
	{
		for (int num = m_types.Length - 1; num >= 0; num--)
		{
			if (m_types[num] == 1 || m_types[num] == 5 || m_types[num] == 2 || m_types[num] == 6 || m_types[num] == 7 || m_types[num] == 14)
			{
				m_result[num] = m_types[num];
				m_levels[num] = -1;
			}
			else
			{
				length--;
				m_result[num] = m_result[length];
				m_levels[num] = m_levels[length];
			}
		}
		for (int i = 0; i < m_types.Length; i++)
		{
			if (m_levels[i] == -1)
			{
				if (i == 0)
				{
					m_levels[i] = m_textOrder;
				}
				else
				{
					m_levels[i] = m_levels[i - 1];
				}
			}
		}
	}

	private void CheckNSM(int index, int length, sbyte level, sbyte startType, sbyte endType)
	{
		sbyte b = startType;
		for (int i = index; i < length; i++)
		{
			if (m_result[i] == 13)
			{
				m_result[i] = b;
			}
			else
			{
				b = m_result[i];
			}
		}
		CheckEuropeanDigits(index, length, level, startType, endType);
	}

	private void CheckEuropeanDigits(int index, int length, sbyte level, sbyte startType, sbyte endType)
	{
		for (int i = index; i < length; i++)
		{
			if (m_result[i] != 8)
			{
				continue;
			}
			for (int num = i - 1; num >= index; num--)
			{
				if (m_result[num] == 0 || m_result[num] == 3 || m_result[num] == 4)
				{
					if (m_result[num] == 4)
					{
						m_result[i] = 11;
					}
					break;
				}
			}
		}
		CheckArabicCharacters(index, length, level, startType, endType);
	}

	private void CheckArabicCharacters(int index, int length, sbyte level, sbyte startType, sbyte endType)
	{
		for (int i = index; i < length; i++)
		{
			if (m_result[i] == 4)
			{
				m_result[i] = 3;
			}
		}
		CheckEuropeanNumberSeparator(index, length, level, startType, endType);
	}

	private void CheckEuropeanNumberSeparator(int index, int length, sbyte level, sbyte startType, sbyte endType)
	{
		for (int i = index + 1; i < length - 1; i++)
		{
			if (m_result[i] == 9 || m_result[i] == 12)
			{
				sbyte b = m_result[i - 1];
				sbyte b2 = m_result[i + 1];
				if (b == 8 && b2 == 8)
				{
					m_result[i] = 8;
				}
				else if (m_result[i] == 12 && b == 11 && b2 == 11)
				{
					m_result[i] = 11;
				}
			}
		}
		CheckEuropeanNumberTerminator(index, length, level, startType, endType);
	}

	private void CheckEuropeanNumberTerminator(int index, int length, sbyte level, sbyte startType, sbyte endType)
	{
		for (int i = index; i < length; i++)
		{
			if (m_result[i] != 10)
			{
				continue;
			}
			int num = i;
			int length2 = GetLength(num, length, new sbyte[1] { 10 });
			sbyte b = ((num == index) ? startType : m_result[num - 1]);
			if (b != 8)
			{
				b = ((length2 == length) ? endType : m_result[length2]);
			}
			if (b == 8)
			{
				for (int j = num; j < length2; j++)
				{
					m_result[j] = 8;
				}
			}
			i = length2;
		}
		CheckOtherNeutrals(index, length, level, startType, endType);
	}

	private void CheckOtherNeutrals(int index, int length, sbyte level, sbyte startType, sbyte endType)
	{
		for (int i = index; i < length; i++)
		{
			if (m_result[i] == 9 || m_result[i] == 10 || m_result[i] == 12)
			{
				m_result[i] = 18;
			}
		}
		CheckOtherCharacters(index, length, level, startType, endType);
	}

	private void CheckOtherCharacters(int index, int length, sbyte level, sbyte startType, sbyte endType)
	{
		for (int i = index; i < length; i++)
		{
			if (m_result[i] != 8)
			{
				continue;
			}
			sbyte b = startType;
			for (int num = i - 1; num >= index; num--)
			{
				if (m_result[num] == 0 || m_result[num] == 3)
				{
					b = m_result[num];
					break;
				}
			}
			if (b == 0)
			{
				m_result[i] = 0;
			}
		}
		CheckCommanCharacters(index, length, level, startType, endType);
	}

	private int GetLength(int index, int length, sbyte[] validSet)
	{
		index--;
		if (++index < length)
		{
			sbyte b = m_result[index];
			for (int i = 0; i < validSet.Length; i++)
			{
				if (b == validSet[i])
				{
					index = GetLength(++index, length, validSet);
				}
			}
			return index;
		}
		return length;
	}

	private void CheckCommanCharacters(int index, int length, sbyte level, sbyte startType, sbyte endType)
	{
		for (int i = index; i < length; i++)
		{
			if (m_result[i] != 17 && m_result[i] != 18 && m_result[i] != 15 && m_result[i] != 16)
			{
				continue;
			}
			int num = i;
			int length2 = GetLength(num, length, new sbyte[4] { 15, 16, 17, 18 });
			sbyte b;
			if (num == index)
			{
				b = startType;
			}
			else
			{
				b = m_result[num - 1];
				switch (b)
				{
				case 11:
					b = 3;
					break;
				case 8:
					b = 3;
					break;
				}
			}
			sbyte b2;
			if (length2 == length)
			{
				b2 = endType;
			}
			else
			{
				b2 = m_result[length2];
				switch (b2)
				{
				case 11:
					b2 = 3;
					break;
				case 8:
					b2 = 3;
					break;
				}
			}
			sbyte b3 = (sbyte)((b != b2) ? ((((uint)level & (true ? 1u : 0u)) != 0) ? 3 : 0) : b);
			for (int j = num; j < length2; j++)
			{
				m_result[j] = b3;
			}
			i = length2;
		}
	}
}
