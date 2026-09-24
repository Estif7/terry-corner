-- ==============================================================================
-- TERRY CORNER - SUPABASE SCHEMA & INITIAL SEED DATA
-- Run this entire script in the Supabase Dashboard -> SQL Editor -> New Query
-- ==============================================================================

-- 1. EXTENSIONS
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 2. CATEGORIES TABLE
CREATE TABLE IF NOT EXISTS public.categories (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    description TEXT,
    image_url TEXT,
    sort_order INT DEFAULT 0,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- 3. TOPPINGS TABLE
CREATE TABLE IF NOT EXISTS public.toppings (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    additional_price NUMERIC(10, 2) NOT NULL DEFAULT 0,
    is_available BOOLEAN DEFAULT true,
    sort_order INT DEFAULT 0,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- 4. PRODUCTS TABLE
CREATE TABLE IF NOT EXISTS public.products (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    description TEXT,
    price NUMERIC(10, 2) NOT NULL DEFAULT 0,
    image_url TEXT,
    category_id TEXT REFERENCES public.categories(id) ON DELETE SET NULL,
    is_available BOOLEAN DEFAULT true,
    is_featured BOOLEAN DEFAULT false,
    is_popular BOOLEAN DEFAULT false,
    sort_order INT DEFAULT 0,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- 5. PRODUCT TOPPINGS (JOIN TABLE)
CREATE TABLE IF NOT EXISTS public.product_toppings (
    product_id TEXT REFERENCES public.products(id) ON DELETE CASCADE,
    topping_id TEXT REFERENCES public.toppings(id) ON DELETE CASCADE,
    PRIMARY KEY (product_id, topping_id)
);

-- 6. PROMOTIONS TABLE
CREATE TABLE IF NOT EXISTS public.promotions (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    description TEXT,
    discount_type TEXT,
    discount_value NUMERIC(10, 2) DEFAULT 0,
    is_featured BOOLEAN DEFAULT true,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- 7. ROW LEVEL SECURITY (RLS) POLICIES
-- Enable RLS
ALTER TABLE public.categories ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.toppings ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.products ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.product_toppings ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.promotions ENABLE ROW LEVEL SECURITY;

-- Anonymous public read policies (allows customers on Vercel to view menu & prices)
CREATE POLICY "Public Read Categories" ON public.categories FOR SELECT USING (true);
CREATE POLICY "Public Read Toppings" ON public.toppings FOR SELECT USING (true);
CREATE POLICY "Public Read Products" ON public.products FOR SELECT USING (true);
CREATE POLICY "Public Read Product Toppings" ON public.product_toppings FOR SELECT USING (true);
CREATE POLICY "Public Read Promotions" ON public.promotions FOR SELECT USING (true);

-- Authenticated admin full access policies (allows logged-in admin to insert, update, delete)
CREATE POLICY "Admin Full Categories" ON public.categories FOR ALL TO authenticated USING (true) WITH CHECK (true);
CREATE POLICY "Admin Full Toppings" ON public.toppings FOR ALL TO authenticated USING (true) WITH CHECK (true);
CREATE POLICY "Admin Full Products" ON public.products FOR ALL TO authenticated USING (true) WITH CHECK (true);
CREATE POLICY "Admin Full Product Toppings" ON public.product_toppings FOR ALL TO authenticated USING (true) WITH CHECK (true);
CREATE POLICY "Admin Full Promotions" ON public.promotions FOR ALL TO authenticated USING (true) WITH CHECK (true);

-- 8. STORAGE BUCKET FOR MEAL IMAGES
INSERT INTO storage.buckets (id, name, public)
VALUES ('meals', 'meals', true)
ON CONFLICT (id) DO UPDATE SET public = true;

-- Allow public read of uploaded meal images
CREATE POLICY "Public Read Meals Storage" ON storage.objects
FOR SELECT USING (bucket_id = 'meals');

-- Allow authenticated admin to upload meal images
CREATE POLICY "Admin Upload Meals Storage" ON storage.objects
FOR INSERT TO authenticated WITH CHECK (bucket_id = 'meals');

CREATE POLICY "Admin Update Meals Storage" ON storage.objects
FOR UPDATE TO authenticated USING (bucket_id = 'meals');

CREATE POLICY "Admin Delete Meals Storage" ON storage.objects
FOR DELETE TO authenticated USING (bucket_id = 'meals');

-- ==============================================================================
-- 9. PRE-SEED INITIAL DATA (TERRY CORNER AUTHENTIC MENU)
-- ==============================================================================

-- Seed Categories
INSERT INTO public.categories (id, name, description, image_url, sort_order)
VALUES ('cat-burgers', 'Burgers', 'Juicy, flame-grilled beef & chicken burgers served in soft toasted buns.', '/meals/special-burger.png', 1)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  image_url = EXCLUDED.image_url,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.categories (id, name, description, image_url, sort_order)
VALUES ('cat-pizzas', 'Pizzas', 'Oven-baked crusts loaded with rich tomato sauce, melted mozzarella, and fresh toppings.', '/meals/special-pizza.png', 2)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  image_url = EXCLUDED.image_url,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.categories (id, name, description, image_url, sort_order)
VALUES ('cat-sandwiches', 'Sandwiches', 'Freshly toasted signature subs and stacked sandwiches with house sauce.', '/meals/special-sandwich.png', 3)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  image_url = EXCLUDED.image_url,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.categories (id, name, description, image_url, sort_order)
VALUES ('cat-habesha', 'Traditional (ሀገረኛ)', 'Hearty Ethiopian breakfast favorites, tibs, shiro, pasta, and firfir prepared with authentic spiced butter.', '/meals/ethiopian-tibs.png', 4)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  image_url = EXCLUDED.image_url,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.categories (id, name, description, image_url, sort_order)
VALUES ('cat-sides', 'Sides & Extras', 'Crispy French fries, extra toppings, and takeaway packaging.', '/meals/french-fries.png', 5)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  image_url = EXCLUDED.image_url,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.categories (id, name, description, image_url, sort_order)
VALUES ('cat-drinks', 'Drinks', 'Chilled soft drinks, mineral water, freshly brewed coffee, and spiced tea.', '/meals/soft-drinks.png', 6)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  image_url = EXCLUDED.image_url,
  sort_order = EXCLUDED.sort_order;

-- Seed Toppings
INSERT INTO public.toppings (id, name, additional_price, is_available)
VALUES ('top-egg', 'Egg', 50, true)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  additional_price = EXCLUDED.additional_price,
  is_available = EXCLUDED.is_available;
INSERT INTO public.toppings (id, name, additional_price, is_available)
VALUES ('top-cheese', 'Cheese', 150, true)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  additional_price = EXCLUDED.additional_price,
  is_available = EXCLUDED.is_available;
INSERT INTO public.toppings (id, name, additional_price, is_available)
VALUES ('top-tuna', 'Tuna', 160, true)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  additional_price = EXCLUDED.additional_price,
  is_available = EXCLUDED.is_available;
INSERT INTO public.toppings (id, name, additional_price, is_available)
VALUES ('top-beef', 'Cheese / Beef', 100, true)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  additional_price = EXCLUDED.additional_price,
  is_available = EXCLUDED.is_available;

-- Seed Products
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-classic-burger', 'Classic Burger', 'Beef patty, lettuce, tomato, onion & house sauce, served in a soft bun.', 460, '/meals/classic-burger.png', 'cat-burgers', true, true, true, 1)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-cheese-burger', 'Cheese Burger', 'Beef patty, double cheese, lettuce, tomato, onion & Terry sauce, served in a soft bun.', 480, '/meals/cheese-burger.png', 'cat-burgers', true, false, true, 2)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-chicken-burger', 'Chicken Burger', 'Chicken breast, creamy mayo, cheese, lettuce & egg, served in a soft bun.', 550, '/meals/chicken-burger.png', 'cat-burgers', true, true, false, 3)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-double-burger', 'Double Burger', 'Double beef patties, egg, cheese & special sauce, served in a soft bun.', 590, '/meals/double-burger.jpg', 'cat-burgers', true, true, true, 4)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-jumbo-burger', 'Jumbo Burger', 'Triple beef patties, cheese, sliced beef, egg, lettuce, mayo, homemade sauce, served in a soft toasted bun.', 750, '/meals/jumbo-burger-hd.png', 'cat-burgers', true, true, true, 5)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-special-burger', 'Terry Special Burger', 'Double beef patties, double cheese, sliced beef, double egg, lettuce, homemade BBQ sauce & mayo, served in a soft bun.', 650, '/meals/special-burger.png', 'cat-burgers', true, true, true, 6)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-margherita-pizza', 'Margherita Pizza', 'Tomato sauce, mozzarella & fresh basil.', 570, '/meals/margherita-pizza.png', 'cat-pizzas', true, false, false, 7)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-chicken-pizza', 'Chicken Pizza', 'Tomato sauce, mozzarella, seasoned chicken, onion, boiled egg.', 590, '/meals/chicken-pizza.png', 'cat-pizzas', true, false, true, 8)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-beef-pizza', 'Beef Pizza', 'Tomato sauce, mozzarella, seasoned beef, onion, boiled egg.', 580, '/meals/beef-pizza.png', 'cat-pizzas', true, false, true, 9)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-tuna-pizza', 'Tuna Pizza', 'Tomato sauce, mozzarella and flaky tuna.', 580, '/meals/tuna-pizza.png', 'cat-pizzas', true, false, false, 10)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-veggie-pizza', 'Veggie Pizza', 'Tomato sauce, mixed vegetables, mushrooms, olives & special spices.', 450, '/meals/veggie-pizza.png', 'cat-pizzas', true, false, false, 11)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-half-and-half-pizza', 'Half & Half Pizza', 'Choose any two pizza flavors and enjoy your favorites on one pizza. You choose the flavor for each half!', 690, '/meals/half-and-half-pizza.png', 'cat-pizzas', true, true, true, 12)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-special-pizza', 'Terry Special Pizza', 'Oven-baked dough, tomato sauce, mozzarella, beef, tuna, chicken, mushrooms, sliced beef, boiled egg, olives & special spices.', 650, '/meals/special-pizza.png', 'cat-pizzas', true, true, true, 13)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-chicken-sandwich', 'Chicken Sandwich', 'Chicken, lettuce, tomato, onion & signature sauce.', 410, '/meals/chicken-club-sandwich.png', 'cat-sandwiches', true, false, true, 14)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-beef-sandwich', 'Beef Sandwich', 'Seasoned beef, lettuce, tomato, onion & signature sauce.', 350, '/meals/beef-sandwich.png', 'cat-sandwiches', true, false, false, 15)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-tuna-sandwich', 'Tuna Sandwich', 'Tuna, lettuce, tomato, onion & creamy sauce.', 350, '/meals/tuna-sandwich.png', 'cat-sandwiches', true, false, false, 16)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-egg-sandwich', 'Egg Sandwich', 'Egg, lettuce, tomato & sauce.', 300, '/meals/egg-sandwich.png', 'cat-sandwiches', true, false, false, 17)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-club-sandwich', 'Club Sandwich', 'Tuna, lettuce, tomato, special sauce, egg & spices.', 450, '/meals/club-sandwich.png', 'cat-sandwiches', true, false, true, 18)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-special-sandwich', 'Terry Special Sandwich', 'Grilled chicken, beef, cheese, lettuce, tomato, onion, house sauce.', 450, '/meals/special-sandwich.png', 'cat-sandwiches', true, true, true, 19)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-chechebsa-butter', 'ጨጨብሳ በቅቤ (Chechebsa)', 'Torn flatbread prepared with pure spiced butter (kibe) and natural honey.', 270, '/meals/chechebsa-with-honey.png', 'cat-habesha', true, true, true, 20)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-chechebsa-egg', 'ጨጨብሳ በቅቤ እና እንቁላል', 'Traditional spiced butter chechebsa topped with sautéed egg.', 310, '/meals/egg-chechebsa.png', 'cat-habesha', true, false, true, 21)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-scrambled-eggs', 'እንቁላል ፍርፍር (Scrambled Eggs)', 'Scrambled eggs seasoned with onions, tomatoes, and green peppers.', 270, '/meals/scrambled-eggs.png', 'cat-habesha', true, false, false, 22)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-scrambled-eggs-beef', 'እንቁላል በስጋ', 'Hearty scrambled eggs cooked with seasoned minced beef and spices.', 310, '/meals/scrambled-eggs-with-beef.png', 'cat-habesha', true, false, true, 23)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-ful-fasting', 'ፉል የጾም (Fasting Ful)', 'Slow-simmered fava beans seasoned with tomato, onion, and vegetable oil.', 200, '/meals/ethiopian-ful.png', 'cat-habesha', true, false, false, 24)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-ful-special', 'ፉል ስፔሻል (Special Ful)', 'Fava beans garnished with boiled egg, yogurt, cheese, fresh tomatoes & onions.', 270, '/meals/ethiopian-ful-special.png', 'cat-habesha', true, false, true, 25)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-dulet', 'ዱለት (Dulet)', 'Finely minced seasoned tripe, beef, and liver cooked with spiced butter and mitmita.', 420, '/meals/dulet.png', 'cat-habesha', true, false, true, 26)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-fetira-butter', 'ፈቲራ በቅቤ (Fetira with Butter)', 'Crispy layered pastry served warm with spiced butter and honey.', 270, '/meals/fetira-with-honey.png', 'cat-habesha', true, false, false, 27)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-fetira-egg', 'ፈቲራ በቅቤ እና እንቁላል', 'Crisp golden fetira layered with egg, spiced butter, and pure honey.', 330, '/meals/fetira-with-egg-and-honey.png', 'cat-habesha', true, false, true, 28)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-firfir', 'ፍርፍር (Firfir)', 'Torn injera simmered in a flavorful berbere sauce with onions and spices.', 250, '/meals/ferfer.png', 'cat-habesha', true, false, false, 29)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-tibs', 'ጥብስ (Tibs)', 'Pan-sautéed tender beef cubes with onions, rosemary, garlic, and jalapeños.', 450, '/meals/ethiopian-tibs.png', 'cat-habesha', true, true, true, 30)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-quanta-firfir', 'ጥብስ ፍርፍር / ቋንጣ ፍርፍር', 'Dried spiced beef (quanta) sautéed in rich berbere sauce and tossed with injera.', 380, '/meals/quanta-ferfer.png', 'cat-habesha', true, false, true, 31)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-pasta-beef', 'ፓስታ በስጋ (Pasta with Beef)', 'Pasta tossed with slow-cooked savory minced beef bolognese sauce.', 350, '/meals/pasta-with-beef.png', 'cat-habesha', true, false, true, 32)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-veggie-pasta', 'ፓስታ በአትክልት (Veggie Pasta)', 'Pasta tossed with sautéed seasonal vegetables and herbs in tomato sauce.', 300, '/meals/veggie-pasta.png', 'cat-habesha', true, false, false, 33)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-pasta-sauce', 'ፓስታ በሶስ (Pasta with Sauce)', 'Classic pasta served with our signature spiced house tomato sauce.', 270, '/meals/pasta.png', 'cat-habesha', true, false, false, 34)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-shiro', 'ሽሮ (Shiro)', 'Smooth chickpea and split-pea flour stew cooked with garlic, ginger, and berbere.', 250, '/meals/ethiopian-shiro.png', 'cat-habesha', true, false, false, 35)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-tegabino', 'ተጋቢኖ (Tegabino Shiro)', 'Thick, bubbling shiro served hot in a traditional clay pot.', 300, '/meals/ethiopian-tegabino.png', 'cat-habesha', true, false, true, 36)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-bozena-shiro', 'ቦዘና ሽሮ (Bozena Shiro)', 'Rich shiro stew enriched with tender seasoned beef chunks.', 380, '/meals/ethiopian-bozena-shiro.png', 'cat-habesha', true, true, true, 37)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-french-fries', 'French Fries', 'Golden, crispy, hot seasoned potato fries.', 300, '/meals/french-fries.png', 'cat-sides', true, false, true, 38)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-takeaway-pizza-box', 'Takeaway Pizza Box', 'Sturdy takeaway box for your pizza orders.', 60, '/meals/margherita-pizza.png', 'cat-sides', true, false, false, 39)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-takeaway-burger-box', 'Takeaway Burger Box', 'Takeaway box container for burgers and meals.', 50, '/meals/classic-burger.png', 'cat-sides', true, false, false, 40)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-ketchup-box', 'Ketchup Box', 'Extra condiment sauce portion.', 10, '/meals/french-fries.png', 'cat-sides', true, false, false, 41)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-soft-drinks', 'Soft Drinks', 'Chilled Coca-Cola, Fanta, Sprite, or Mirinda.', 60, '/meals/soft-drinks.png', 'cat-drinks', true, false, true, 42)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-water', '1/2 Liter Water', 'Bottled pure mineral spring water (500ml).', 50, '/meals/soft-drinks.png', 'cat-drinks', true, false, false, 43)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-coffee', 'Coffee', 'Freshly brewed traditional Ethiopian roasted coffee.', 40, '/meals/soft-drinks.png', 'cat-drinks', true, false, false, 44)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;
INSERT INTO public.products (id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order)
VALUES ('prod-tea', 'Tea', 'Hot spiced Ethiopian tea with aromatic spices.', 30, '/meals/soft-drinks.png', 'cat-drinks', true, false, false, 45)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  price = EXCLUDED.price,
  image_url = EXCLUDED.image_url,
  category_id = EXCLUDED.category_id,
  is_available = EXCLUDED.is_available,
  is_featured = EXCLUDED.is_featured,
  is_popular = EXCLUDED.is_popular,
  sort_order = EXCLUDED.sort_order;

-- Seed Product Toppings Relationships
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-classic-burger', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-classic-burger', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-classic-burger', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-classic-burger', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-cheese-burger', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-cheese-burger', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-cheese-burger', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-cheese-burger', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chicken-burger', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chicken-burger', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chicken-burger', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chicken-burger', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-double-burger', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-double-burger', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-double-burger', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-double-burger', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-jumbo-burger', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-jumbo-burger', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-jumbo-burger', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-jumbo-burger', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-special-burger', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-special-burger', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-special-burger', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-special-burger', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-margherita-pizza', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-margherita-pizza', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-margherita-pizza', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-margherita-pizza', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chicken-pizza', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chicken-pizza', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chicken-pizza', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chicken-pizza', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-beef-pizza', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-beef-pizza', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-beef-pizza', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-beef-pizza', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-tuna-pizza', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-tuna-pizza', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-tuna-pizza', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-tuna-pizza', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-veggie-pizza', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-veggie-pizza', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-veggie-pizza', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-veggie-pizza', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-half-and-half-pizza', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-half-and-half-pizza', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-half-and-half-pizza', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-half-and-half-pizza', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-special-pizza', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-special-pizza', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-special-pizza', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-special-pizza', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chicken-sandwich', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chicken-sandwich', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chicken-sandwich', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chicken-sandwich', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-beef-sandwich', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-beef-sandwich', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-beef-sandwich', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-beef-sandwich', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-tuna-sandwich', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-tuna-sandwich', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-tuna-sandwich', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-tuna-sandwich', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-egg-sandwich', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-egg-sandwich', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-egg-sandwich', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-egg-sandwich', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-club-sandwich', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-club-sandwich', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-club-sandwich', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-club-sandwich', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-special-sandwich', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-special-sandwich', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-special-sandwich', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-special-sandwich', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chechebsa-butter', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chechebsa-butter', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chechebsa-butter', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chechebsa-butter', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chechebsa-egg', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chechebsa-egg', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chechebsa-egg', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-chechebsa-egg', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-scrambled-eggs', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-scrambled-eggs', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-scrambled-eggs', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-scrambled-eggs', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-scrambled-eggs-beef', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-scrambled-eggs-beef', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-scrambled-eggs-beef', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-scrambled-eggs-beef', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-ful-fasting', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-ful-fasting', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-ful-fasting', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-ful-fasting', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-ful-special', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-ful-special', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-ful-special', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-ful-special', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-dulet', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-dulet', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-dulet', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-dulet', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-fetira-butter', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-fetira-butter', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-fetira-butter', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-fetira-butter', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-fetira-egg', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-fetira-egg', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-fetira-egg', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-fetira-egg', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-firfir', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-firfir', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-firfir', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-firfir', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-tibs', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-tibs', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-tibs', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-tibs', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-quanta-firfir', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-quanta-firfir', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-quanta-firfir', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-quanta-firfir', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-pasta-beef', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-pasta-beef', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-pasta-beef', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-pasta-beef', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-veggie-pasta', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-veggie-pasta', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-veggie-pasta', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-veggie-pasta', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-pasta-sauce', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-pasta-sauce', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-pasta-sauce', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-pasta-sauce', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-shiro', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-shiro', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-shiro', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-shiro', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-tegabino', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-tegabino', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-tegabino', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-tegabino', 'top-beef')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-bozena-shiro', 'top-egg')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-bozena-shiro', 'top-cheese')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-bozena-shiro', 'top-tuna')
ON CONFLICT DO NOTHING;
INSERT INTO public.product_toppings (product_id, topping_id)
VALUES ('prod-bozena-shiro', 'top-beef')
ON CONFLICT DO NOTHING;

-- Seed Promotions
INSERT INTO public.promotions (id, name, description, discount_type, discount_value, is_featured)
VALUES ('promo-1', 'Terry Special Crave Deal', 'Terry Special Burger with double beef, double cheese, sliced beef & double egg for only ETB 650!', 'FixedAmount', 150, true)
ON CONFLICT (id) DO UPDATE SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  discount_type = EXCLUDED.discount_type,
  discount_value = EXCLUDED.discount_value,
  is_featured = EXCLUDED.is_featured;
