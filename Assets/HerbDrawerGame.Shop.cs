using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public sealed partial class HerbDrawerGame
{
    private const int CustomerCount = 5;
    private readonly List<Material> shopMaterials = new List<Material>();
    private readonly Transform[] customerModels = new Transform[CustomerCount];
    private readonly Transform[] drawerModels = new Transform[18];
    private readonly Transform[] drawerFronts = new Transform[18];
    private readonly Transform[] drawerHandles = new Transform[18];
    private readonly MeshRenderer[] drawerTags = new MeshRenderer[18];
    private readonly Transform[] drawerLabels = new Transform[18];
    private Camera shopCamera;
    private bool faceCabinet;
    private Material woodMaterial;
    private Material darkWood;
    private Material brass;
    private Material paper;
    private Material selectedPaper;
    private Material filledPaper;

    private void BuildShop()
    {
        shopCamera = Camera.main;
        if (shopCamera == null)
            shopCamera = new GameObject("阿嬤的視角").AddComponent<Camera>();
        shopCamera.transform.SetPositionAndRotation(new Vector3(0, 1.62f, 0), Quaternion.Euler(5, 0, 0));
        shopCamera.fieldOfView = 60;
        shopCamera.nearClipPlane = 0.06f;
        shopCamera.clearFlags = CameraClearFlags.SolidColor;
        shopCamera.backgroundColor = new Color32(48, 28, 21, 255);

        woodMaterial = Finish(new Color32(152, 107, 71, 255), wood);
        darkWood = Finish(new Color32(74, 39, 25, 255));
        brass = Finish(new Color32(209, 141, 56, 255));
        paper = Finish(new Color32(244, 209, 151, 255));
        selectedPaper = Finish(new Color32(255, 177, 50, 255));
        filledPaper = Finish(new Color32(141, 183, 111, 255));
        BuildRoom();
        BuildCabinet();
        BuildCustomers();
        BuildOwnerHands();
    }

    private void BuildRoom()
    {
        Transform room = Group("迪化街中藥鋪", transform, Vector3.zero);
        Material plaster = Finish(new Color32(195, 155, 113, 255));
        Material brick = Finish(new Color32(142, 67, 46, 255));
        Material alternateBrick = Finish(new Color32(168, 85, 58, 255));
        Material floor = Finish(new Color32(117, 77, 51, 255));
        Material alternateFloor = Finish(new Color32(132, 87, 55, 255));
        Material window = Finish(new Color32(127, 179, 173, 255));
        Material shade = Finish(new Color32(245, 168, 89, 255));
        Material ceramic = Finish(new Color32(80, 128, 119, 255));
        Material creamJar = Finish(new Color32(221, 195, 153, 255));

        Box(room, "Ceiling", new Vector3(0, 3.67f, 0.7f), new Vector3(9, 0.2f, 11), plaster);
        Box(room, "Back wall", new Vector3(0, 1.8f, -4.15f), new Vector3(9, 3.7f, 0.2f), plaster);
        Box(room, "Street wall", new Vector3(0, 1.8f, 5.8f), new Vector3(9, 3.7f, 0.25f), brick);
        for (int side = -1; side <= 1; side += 2)
        {
            Box(room, "Plaster wall", new Vector3(4.45f * side, 1.8f, 0.7f), new Vector3(.2f, 3.7f, 10), plaster);
            Box(room, "Wainscot", new Vector3(4.32f * side, .47f, .7f), new Vector3(.12f, .94f, 9.8f), darkWood);
            for (int i = -2; i <= 2; i++)
                Box(room, "Wall post", new Vector3(4.21f * side, 1.82f, i * 2), new Vector3(.22f, 3.65f, .18f), woodMaterial);
        }
        for (int i = 0; i < 24; i++)
            Box(room, "Floorboard", new Vector3(-4.32f + i * .375f, -.075f, .7f), new Vector3(.371f, .15f, 10.1f), i % 3 == 0 ? alternateFloor : floor);
        for (int row = 0; row < 12; row++)
        {
            for (int col = 0; col < 18; col++)
            {
                float x = -4.24f + col * .48f + row % 2 * .24f;
                Box(room, "Red brick", new Vector3(x, .17f + row * .29f, 5.64f),
                    new Vector3(.455f, .267f, .11f), (row + col) % 4 == 0 ? alternateBrick : brick);
            }
        }
        for (int i = -1; i <= 1; i++)
        {
            float x = i * 2.55f;
            Box(room, "Window frame", new Vector3(x, 1.78f, 5.48f), new Vector3(1.74f, 2.8f, .15f), darkWood);
            Box(room, "Frosted pane", new Vector3(x, 1.78f, 5.36f), new Vector3(1.54f, 2.55f, .12f), window);
            for (int j = -1; j <= 1; j++)
                Box(room, "Window grille", new Vector3(x + j * .42f, 1.78f, 5.27f), new Vector3(.047f, 2.56f, .07f), darkWood);
            Box(room, "Window transom", new Vector3(x, 2.34f, 5.25f), new Vector3(1.57f, .09f, .09f), darkWood);
        }

        Box(room, "Counter", new Vector3(0, .48f, 1.12f), new Vector3(6.75f, .96f, .86f), darkWood);
        Box(room, "Counter top", new Vector3(0, 1.01f, 1.12f), new Vector3(6.96f, .15f, 1.04f), woodMaterial);
        for (int i = -3; i <= 3; i++)
            Box(room, "Counter panel", new Vector3(i * .93f, .49f, .67f), new Vector3(.84f, .72f, .04f), woodMaterial);

        for (int side = -1; side <= 1; side += 2)
        {
            float x = 3.48f * side;
            for (int level = 0; level < 3; level++)
            {
                float y = .97f + level * .63f;
                Box(room, "Jar shelf", new Vector3(x, y, -3.66f), new Vector3(1.28f, .09f, .66f), darkWood);
                for (int i = 0; i < 3; i++)
                {
                    Vector3 jar = new Vector3(x + (i - 1) * .37f, y + .23f, -3.61f);
                    Shape(room, "Ceramic jar", PrimitiveType.Cylinder, jar, new Vector3(.29f, .18f, .29f), i % 2 == 0 ? ceramic : creamJar);
                    Shape(room, "Jar lid", PrimitiveType.Cylinder, jar + Vector3.up * .19f, new Vector3(.31f, .041f, .31f), darkWood);
                    Box(room, "Jar seal", jar + new Vector3(0, .01f, .145f), new Vector3(.12f, .15f, .018f), paper);
                }
            }
            Box(room, "Ceiling beam", new Vector3(1.9f * side, 3.49f, .7f), new Vector3(.22f, .25f, 10), darkWood);
            for (int i = -1; i <= 1; i++)
            {
                Vector3 lamp = new Vector3(side * 2.3f, 3.05f, i * 3.2f);
                Box(room, "Lantern cord", lamp + Vector3.up * .36f, new Vector3(.037f, .42f, .037f), darkWood);
                Shape(room, "Fabric lantern", PrimitiveType.Sphere, lamp, new Vector3(.38f, .48f, .38f), shade);
            }
        }

        Light fill = Group("室內暖光", room, Vector3.zero).gameObject.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.color = new Color32(255, 223, 184, 255);
        fill.intensity = 1.35f;
        fill.transform.rotation = Quaternion.Euler(35, -145, 0);
    }

    private void BuildCabinet()
    {
        Transform cabinet = Group("百草藥櫃", transform, new Vector3(0, 0, -3.53f));
        Box(cabinet, "Cabinet carcass", new Vector3(0, 1.52f, -.2f), new Vector3(5.13f, 2.83f, .59f), darkWood);
        Box(cabinet, "Cabinet crown", new Vector3(0, 2.91f, .11f), new Vector3(5.35f, .17f, .72f), woodMaterial);
        Box(cabinet, "Plinth", new Vector3(0, .39f, .11f), new Vector3(5.27f, .23f, .75f), woodMaterial);
        Box(cabinet, "Carved sign", new Vector3(0, 2.67f, .15f), new Vector3(2.4f, .3f, .09f), brass);
        AddHerbsSign(cabinet);
        Material trayLining = Finish(new Color32(207, 170, 117, 255));
        for (int i = 0; i < drawers.Length; i++)
        {
            Transform drawer = Group(drawers[i] + "抽屜", cabinet, new Vector3(
                (2.5f - i % Columns) * .8f, 2.25f - i / Columns * .68f, .1f));
            drawerModels[i] = drawer;
            Transform front = Group("Drawer face", drawer, Vector3.zero);
            drawerFronts[i] = front;
            Box(front, "Drawer front", new Vector3(0, 0, .035f), new Vector3(.755f, .6f, .12f), woodMaterial);
            drawerTags[i] = Box(front, "Label plate", new Vector3(0, .095f, .105f), new Vector3(.44f, .20f, .025f), paper);
            drawerLabels[i] = Group("Label anchor", front, new Vector3(0, .095f, .124f));
            Transform handle = Group("Brass pull", front, new Vector3(0, -.135f, .167f));
            drawerHandles[i] = handle;
            Box(handle, "Grip bar", Vector3.zero, new Vector3(.17f, .028f, .028f), brass);
            for (int side = -1; side <= 1; side += 2)
                Box(handle, "Grip mount", new Vector3(side * .07f, 0, -.035f), new Vector3(.025f, .044f, .079f), brass);

            // The shallow internal tray slopes toward the player so even the top row is visible.
            Transform tray = Group("藥材托盤", drawer, new Vector3(0, -.07f, -.28f));
            tray.localRotation = Quaternion.Euler(34, 0, 0);
            Box(tray, "Tray base", Vector3.zero, new Vector3(.72f, .065f, .63f), darkWood);
            Box(tray, "Lining", Vector3.up * .042f, new Vector3(.65f, .017f, .57f), trayLining);
            for (int side = -1; side <= 1; side += 2)
                Box(tray, "Side rail", new Vector3(side * .345f, .055f, 0), new Vector3(.035f, .095f, .63f), woodMaterial);
            Box(tray, "Back rail", new Vector3(0, .055f, -.3f), new Vector3(.69f, .095f, .035f), woodMaterial);
            Box(tray, "Front lip", new Vector3(0, .034f, .3f), new Vector3(.69f, .07f, .035f), woodMaterial);
            BuildHerbPile(tray, drawers[i]);
        }
    }

    private Transform herbsSign;

    private void AddHerbsSign(Transform cabinet)
    {
        herbsSign = Group("百草櫃匾額", cabinet, new Vector3(0, 2.67f, .205f));
    }

    private void BuildCustomers()
    {
        Color32[] clothes =
        {
            new Color32(56, 99, 107, 255), new Color32(159, 83, 61, 255),
            new Color32(110, 117, 66, 255), new Color32(96, 81, 121, 255),
            new Color32(199, 146, 67, 255)
        };
        Material trousers = Finish(new Color32(57, 53, 52, 255));
        Material hair = Finish(new Color32(51, 37, 29, 255));
        Material greyHair = Finish(new Color32(174, 159, 144, 255));
        Material eyeWhite = Finish(new Color32(228, 217, 202, 255));
        Material iris = Finish(new Color32(67, 43, 33, 255));
        Material pupil = Finish(new Color32(27, 21, 20, 255));
        for (int i = 0; i < CustomerCount; i++)
        {
            Transform customer = Group("第" + (i + 1) + "位客人", transform, QueuePosition(i));
            customerModels[i] = customer;
            float height = 1.02f + i % 3 * .10f;
            Material jacket = Finish(clothes[i]);
            Material skin = SkinFinish(Color.Lerp(new Color32(181, 120, 93, 255), new Color32(226, 168, 134, 255), i % 3 * .42f));
            Material hairColor = i == 3 ? greyHair : hair;
            Shape(customer, "Coat", PrimitiveType.Capsule, new Vector3(0, height - .06f, 0), new Vector3(.54f, .38f, .32f), jacket);
            Shape(customer, "Collar", PrimitiveType.Cylinder, new Vector3(0, height + .26f, 0), new Vector3(.20f, .055f, .19f), paper);
            BuildCustomerFace(customer, height, i, skin, hairColor, eyeWhite, iris, pupil);
            for (int side = -1; side <= 1; side += 2)
            {
                Shape(customer, "Sleeve", PrimitiveType.Capsule, new Vector3(side * .282f, height - .095f, 0), new Vector3(.155f, .245f, .16f), jacket);
                Shape(customer, "Hand", PrimitiveType.Sphere, new Vector3(side * .29f, height - .34f, -.015f), new Vector3(.12f, .17f, .108f), skin);
                Shape(customer, "Trouser leg", PrimitiveType.Capsule, new Vector3(side * .123f, .38f, 0), new Vector3(.19f, .35f, .19f), trousers);
                Shape(customer, "Shoe", PrimitiveType.Sphere, new Vector3(side * .124f, .09f, -.067f), new Vector3(.20f, .16f, .32f), hair);
            }
            if (i == 0 || i == 4)
                Box(customer, "Shopping bag", new Vector3(.35f, .48f, -.045f), new Vector3(.25f, .36f, .18f), paper);
        }
    }

    private static Vector3 QueuePosition(int slot)
    {
        int offset = (slot + 1) / 2 * (slot % 2 == 0 ? 1 : -1);
        return new Vector3(offset * 1.02f, 0, 2.13f + slot * .43f);
    }

    private void ResetQueue()
    {
        for (int i = 0; i < CustomerCount; i++)
        {
            Transform customer = customerModels[i];
            customer.SetLocalPositionAndRotation(QueuePosition(i), Quaternion.identity);
            customer.gameObject.SetActive(true);
        }
    }

    private void FlipView()
    {
        faceCabinet = !faceCabinet;
    }

    private void UpdateShop()
    {
        float step = Time.unscaledDeltaTime;
        bool lookingAtDrawers = faceCabinet || Time.unscaledTime < finishPullAt;
        Quaternion direction = Quaternion.Euler(lookingAtDrawers ? 0 : 5, lookingAtDrawers ? 180 : 0, 0);
        shopCamera.transform.rotation = Quaternion.RotateTowards(shopCamera.transform.rotation, direction, 540 * step);
        for (int i = 0; i < CustomerCount; i++)
        {
            Vector3 target = i < served ? new Vector3(4.05f, 0, 5.1f) : QueuePosition(i - served);
            Transform customer = customerModels[i];
            customer.localPosition = Vector3.MoveTowards(customer.localPosition, target, 2.8f * step);
            customer.gameObject.SetActive(i >= served || (customer.localPosition - target).sqrMagnitude > .001f);
            customer.localRotation = Quaternion.RotateTowards(customer.localRotation,
                Quaternion.Euler(0, i < served ? -118 : 0, 0), 280 * step);
            float attention = i < served ? 0 : Mathf.Clamp(
                Mathf.Atan2(customer.localPosition.x, customer.localPosition.z) * Mathf.Rad2Deg, -18, 18);
            float tilt = i % 3 == 0 ? -3 : i % 3 == 1 ? 3 : 0;
            Transform head = customerHeads[i];
            head.localRotation = Quaternion.RotateTowards(head.localRotation,
                Quaternion.Euler(i % 2 == 0 ? 1 : -2, attention, tilt), 85 * step);
        }
        for (int i = 0; i < drawers.Length; i++)
        {
            bool revealed = i == opened && Time.unscaledTime < revealUntil;
            float pullProgress = Mathf.SmoothStep(0, 1,
                Mathf.InverseLerp(ReachDuration, PullDuration, Time.unscaledTime - pullStartedAt));
            float extension = revealed ? Mathf.Lerp(.10f, .99f, pullProgress) : collected[i] ? .16f : .10f;
            Transform drawer = drawerModels[i];
            Vector3 position = drawer.localPosition;
            position.z = revealed ? extension : Mathf.MoveTowards(position.z, extension, 3.7f * step);
            drawer.localPosition = position;
            Transform front = drawerFronts[i];
            front.localPosition = Vector3.MoveTowards(front.localPosition,
                Vector3.down * (revealed && pullProgress > .55f ? .47f : 0), 3.4f * step);
            drawerTags[i].sharedMaterial = collected[i] ? filledPaper : IsDrawerSelected(i) ? selectedPaper : paper;
        }
        AnimatePullHand(lookingAtDrawers);
    }

    private void DrawDrawerLabels(Vector2 origin, float scale)
    {
        if (Vector3.Dot(shopCamera.transform.forward, Vector3.back) < .98f)
            return;
        Vector3 sign = shopCamera.WorldToScreenPoint(herbsSign.position);
        Vector2 signPoint = (new Vector2(sign.x, Screen.height - sign.y) - origin) / scale;
        Write(new Rect(signPoint.x - 110, signPoint.y - 18, 220, 36), "百  草  櫃", 22, Ink);
        for (int i = 0; i < drawers.Length; i++)
        {
            Vector3 screen = shopCamera.WorldToScreenPoint(drawerLabels[i].position);
            Vector2 center = (new Vector2(screen.x, Screen.height - screen.y) - origin) / scale;
            Rect nameplate = new Rect(center.x - 35, center.y - 13, 70, 26);
            if (i == opened + Columns && opened >= 0 && drawerModels[opened].localPosition.z > .20f)
                Fill(nameplate, collected[i] ? Green : IsDrawerSelected(i) ? Gold : Cream);
            Write(nameplate, drawers[i], 19, Ink);
        }
    }

    private bool IsDrawerSelected(int index)
    {
        return index == cursor && faceCabinet && phase == Phase.Search;
    }

    private Material Finish(Color color, Texture texture = null)
    {
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color, mainTexture = texture };
        material.SetFloat("_Smoothness", .24f);
        shopMaterials.Add(material);
        return material;
    }

    private static Transform Group(string name, Transform parent, Vector3 position)
    {
        Transform group = new GameObject(name).transform;
        group.SetParent(parent, false);
        group.localPosition = position;
        return group;
    }

    private static MeshRenderer Box(Transform parent, string name, Vector3 position, Vector3 size, Material material)
    {
        return Shape(parent, name, PrimitiveType.Cube, position, size, material);
    }

    private static MeshRenderer Shape(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 size, Material material)
    {
        GameObject model = GameObject.CreatePrimitive(type);
        model.name = name;
        model.transform.SetParent(parent, false);
        model.transform.localPosition = position;
        model.transform.localScale = size;
        Destroy(model.GetComponent<Collider>());
        MeshRenderer renderer = model.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        return renderer;
    }

    private void ReleaseShop()
    {
        foreach (Material material in shopMaterials)
            Destroy(material);
        foreach (Mesh mesh in faceMeshes)
            Destroy(mesh);
        foreach (Mesh mesh in herbMeshes)
            Destroy(mesh);
        foreach (Texture2D texture in herbTextures)
            Destroy(texture);
    }
}
