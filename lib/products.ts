export type Product = {
  id: string;
  slug: string;
  name: string;
  category: string;
  description: string;
  price: number;
  oldPrice?: number;
  sizes: string[];
  colors: string[];
  stock: number;
  badge?: string;
  featured?: boolean;
  image: string;
  accent: string;
};

export const categories = [
  { name: "فساتين سهرة", slug: "evening", count: 24 },
  { name: "فساتين زفاف", slug: "bridal", count: 12 },
  { name: "فساتين يومية", slug: "daily", count: 18 },
  { name: "إطلالات محتشمة", slug: "modest", count: 16 },
];

export const products: Product[] = [
  {
    id: "dress-001",
    slug: "rose-satin-dress",
    name: "فستان روز ساتان",
    category: "evening",
    description: "فستان ساتان بقصة ناعمة ولمعة راقية، مصمم للمناسبات التي تحتاج حضوراً لا يُنسى.",
    price: 485,
    oldPrice: 560,
    sizes: ["S", "M", "L", "XL"],
    colors: ["وردي غباري", "أسود"],
    stock: 8,
    badge: "الأكثر مبيعاً",
    featured: true,
    image: "https://images.unsplash.com/photo-1566174053879-31528523f8ae?auto=format&fit=crop&w=1000&q=85",
    accent: "#e4b5ad",
  },
  {
    id: "dress-002",
    slug: "midnight-velvet",
    name: "مخمل منتصف الليل",
    category: "evening",
    description: "تصميم مخملي بكتف واحد وتفاصيل منحوتة تمنح الإطلالة فخامة هادئة.",
    price: 620,
    sizes: ["S", "M", "L"],
    colors: ["أسود", "أزرق ليلي"],
    stock: 5,
    badge: "جديد",
    featured: true,
    image: "https://images.unsplash.com/photo-1595777457583-95e059d581b8?auto=format&fit=crop&w=1000&q=85",
    accent: "#47415a",
  },
  {
    id: "dress-003",
    slug: "pearl-garden",
    name: "حديقة اللؤلؤ",
    category: "bridal",
    description: "فستان أبيض بتطريزات لؤلؤية خفيفة وتفاصيل رومانسية لعروس عصرية.",
    price: 1450,
    sizes: ["S", "M", "L", "XL"],
    colors: ["أبيض عاجي"],
    stock: 3,
    badge: "مجموعة العرائس",
    featured: true,
    image: "https://images.unsplash.com/photo-1519741497674-611481863552?auto=format&fit=crop&w=1000&q=85",
    accent: "#e9dfce",
  },
  {
    id: "dress-004",
    slug: "olive-drape",
    name: "دريب الزيتون",
    category: "modest",
    description: "قصة طويلة بانسدال أنيق وأكمام واسعة، مناسبة للإطلالات المحتشمة الراقية.",
    price: 390,
    sizes: ["M", "L", "XL", "XXL"],
    colors: ["زيتوني", "بيج"],
    stock: 11,
    featured: true,
    image: "https://images.unsplash.com/photo-1539008835657-9e8e9680c956?auto=format&fit=crop&w=1000&q=85",
    accent: "#a9aa86",
  },
  {
    id: "dress-005",
    slug: "champagne-column",
    name: "عمود الشمبانيا",
    category: "evening",
    description: "فستان بقصة مستقيمة ولمسة لونية دافئة، خيار مثالي لحفلات العشاء والمناسبات.",
    price: 545,
    oldPrice: 640,
    sizes: ["S", "M", "L"],
    colors: ["شمبانيا", "بني داكن"],
    stock: 6,
    badge: "خصم 15%",
    image: "https://images.unsplash.com/photo-1496747611176-843222e1e57c?auto=format&fit=crop&w=1000&q=85",
    accent: "#d9bd9e",
  },
  {
    id: "dress-006",
    slug: "linen-summer",
    name: "نسمة كتان",
    category: "daily",
    description: "فستان كتان خفيف بقصة مريحة وألوان مستوحاة من صباحات الصيف الهادئة.",
    price: 280,
    sizes: ["S", "M", "L", "XL"],
    colors: ["أبيض", "أزرق سماوي"],
    stock: 14,
    badge: "صيف 2026",
    image: "https://images.unsplash.com/photo-1496217590455-aa63a8350eea?auto=format&fit=crop&w=1000&q=85",
    accent: "#c6d9d7",
  },
  {
    id: "dress-007",
    slug: "ruby-pleats",
    name: "طيات الياقوت",
    category: "evening",
    description: "طيات انسيابية ولون ياقوتي غني يرفع أي إطلالة مسائية إلى مستوى آخر.",
    price: 510,
    sizes: ["S", "M", "L", "XL"],
    colors: ["ياقوتي", "بنفسجي"],
    stock: 4,
    image: "https://images.unsplash.com/photo-1485968579580-b6d095142e6e?auto=format&fit=crop&w=1000&q=85",
    accent: "#963e55",
  },
  {
    id: "dress-008",
    slug: "sand-midi",
    name: "ميدي الرمال",
    category: "daily",
    description: "تصميم ميدي عملي بأزرار أمامية وحزام ناعم، للّوك اليومي الأنيق.",
    price: 315,
    sizes: ["S", "M", "L"],
    colors: ["رملي", "أسود"],
    stock: 9,
    image: "https://images.unsplash.com/photo-1525507119028-ed4c629a60a3?auto=format&fit=crop&w=1000&q=85",
    accent: "#c6a986",
  },
];

export function getProduct(slug: string) {
  return products.find((product) => product.slug === slug);
}
