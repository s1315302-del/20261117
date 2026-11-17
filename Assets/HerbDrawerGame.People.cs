using System.Collections.Generic;
using UnityEngine;

public sealed partial class HerbDrawerGame
{
    private readonly List<Mesh> faceMeshes = new List<Mesh>();
    private const float ReachDuration = .25f;
    private const float PullDuration = .58f;
    private const float HandReturnDuration = .83f;
    private readonly Transform[] gripFingers = new Transform[4];
    private Transform rightHand;
    private Transform rightSleeve;
    private Vector3 rightHandRest;
    private Quaternion rightHandRestRotation;

    private Material SkinFinish(Color color)
    {
        Material material = Finish(color);
        material.SetFloat("_Smoothness", .14f);
        return material;
    }

    private void BuildCustomerFace(Transform customer, float height, int person, Material skin,
        Material hair, Material eyeWhite, Material iris, Material pupil)
    {
        Transform head = Group("Head and features", customer, new Vector3(0, height + .47f, -.018f));
        float width = .133f + (person % 3) * .008f;
        float faceHeight = .177f + (person % 2) * .012f;
        float depth = .134f;
        Color skinColor = skin.color;
        Material warmSkin = SkinFinish(Color.Lerp(skinColor, new Color32(158, 82, 70, 255), .22f));
        Material shadowSkin = SkinFinish(Color.Lerp(skinColor, new Color32(90, 43, 36, 255), .43f));
        Material brow = Finish(Color.Lerp(hair.color, new Color32(78, 48, 37, 255), .35f));
        Material upperLip = SkinFinish(Color.Lerp(skinColor, new Color32(140, 68, 65, 255), .55f));
        Material lowerLip = SkinFinish(Color.Lerp(skinColor, new Color32(204, 113, 105, 255), .46f));

        Shape(customer, "Neck", PrimitiveType.Sphere, new Vector3(0, height + .29f, .008f),
            new Vector3(.125f, .19f, .127f), skin);
        BuildHeadSurface(head, "Sculpted face", skin, (polar, azimuth) =>
        {
            Vector3 point = HeadDirection(polar * Mathf.PI, azimuth);
            float jaw = Mathf.Lerp(person % 2 == 0 ? .79f : .72f, 1, Mathf.InverseLerp(-.94f, -.06f, point.y));
            float x = point.x * width * jaw;
            float y = point.y * faceHeight;
            float z = point.z * depth;
            if (point.z < 0)
            {
                z -= FaceVolume(Mathf.Abs(x), y, .061f, -.023f, .034f, .040f) * .009f;
                z -= FaceVolume(x, y, 0, .012f, .021f, .055f) * .020f;
                z -= FaceVolume(x, y, 0, -.026f, .022f, .021f) * .027f;
                z -= FaceVolume(Mathf.Abs(x), y, .016f, -.037f, .013f, .012f) * .011f;
                z += FaceVolume(Mathf.Abs(x), y, .050f, .027f, .030f, .018f) * .006f;
            }
            return new Vector3(x, y, z);
        });

        BuildHeadSurface(head, "Continuous hair silhouette", hair, (polar, azimuth) =>
        {
            float side = Mathf.InverseLerp(.82f, -.85f, Mathf.Cos(azimuth));
            float hairline = Mathf.Lerp(1.03f + person % 3 * .045f, 2.18f, Mathf.SmoothStep(0, 1, side));
            hairline += Mathf.Sin(azimuth * 2) * (person % 2 == 0 ? .082f : -.062f);
            Vector3 point = HeadDirection(hairline * polar, azimuth);
            float contour = Mathf.Sin(polar * Mathf.PI) * Mathf.Sin(azimuth * 11 + polar * 5) * .0018f;
            return new Vector3(point.x * (width + .009f + contour), point.y * (faceHeight + .013f),
                point.z * (depth + .010f + contour));
        });
        if (person == 1 || person == 3)
            Shape(head, "Tied hair", PrimitiveType.Sphere, new Vector3(.04f, .161f, .10f), new Vector3(.132f, .111f, .139f), hair);

        float eyeHeight = .026f;
        for (int side = -1; side <= 1; side += 2)
        {
            float eyeX = side * .051f;
            Shape(head, "Ear", PrimitiveType.Sphere, new Vector3(side * (width - .003f), -.018f, .004f),
                new Vector3(.042f, .071f, .038f), skin).transform.localRotation = Quaternion.Euler(0, 0, -side * 13);
            Shape(head, "Inner ear", PrimitiveType.Sphere, new Vector3(side * (width + .007f), -.015f, -.012f),
                new Vector3(.019f, .044f, .011f), warmSkin);
            Vector3 eye = new Vector3(eyeX, eyeHeight, -.116f);
            Shape(head, "Eye socket", PrimitiveType.Sphere, eye, new Vector3(.048f, .021f, .022f), warmSkin);
            Shape(head, "Eye white", PrimitiveType.Sphere, eye + new Vector3(0, -.001f, -.010f),
                new Vector3(.036f, .011f, .013f), eyeWhite);
            Shape(head, "Iris", PrimitiveType.Sphere, eye + new Vector3(-side * .003f, -.001f, -.016f),
                new Vector3(.014f, .012f, .005f), iris);
            Shape(head, "Pupil", PrimitiveType.Sphere, eye + new Vector3(-side * .003f, -.001f, -.019f),
                new Vector3(.007f, .008f, .003f), pupil);
            Shape(head, "Upper eyelid", PrimitiveType.Sphere, eye + new Vector3(0, .006f, -.010f),
                new Vector3(.038f, .006f, .010f), warmSkin).transform.localRotation = Quaternion.Euler(0, 0, side * 6);
            Shape(head, "Eyebrow", PrimitiveType.Sphere, new Vector3(eyeX, eyeHeight + .028f, -.119f),
                new Vector3(.053f, person == 0 || person == 2 ? .010f : .007f, .015f), brow)
                .transform.localRotation = Quaternion.Euler(0, 0, side * (person == 3 ? -9 : 9));
            Shape(head, "Nostril", PrimitiveType.Sphere, new Vector3(side * .014f, -.043f, -.153f),
                new Vector3(.010f, .005f, .008f), shadowSkin);
        }

        float mouthWidth = .063f + (person % 3) * .007f;
        Shape(head, "Upper lip", PrimitiveType.Sphere, new Vector3(0, -.071f, -.124f),
            new Vector3(mouthWidth, .012f, .017f), upperLip);
        Shape(head, "Lower lip", PrimitiveType.Sphere, new Vector3(0, -.080f, -.124f),
            new Vector3(mouthWidth * .87f, .012f, .018f), lowerLip);
        Shape(head, "Mouth seam", PrimitiveType.Sphere, new Vector3(0, -.075f, -.133f),
            new Vector3(mouthWidth * .78f, .003f, .006f), shadowSkin);

        customerHeads[person] = head;
    }

    private readonly Transform[] customerHeads = new Transform[CustomerCount];

    private static float FaceVolume(float x, float y, float originX, float originY, float width, float height)
    {
        float dx = (x - originX) / width;
        float dy = (y - originY) / height;
        return Mathf.Exp(-(dx * dx + dy * dy));
    }

    private static Vector3 HeadDirection(float polar, float azimuth)
    {
        float radial = Mathf.Sin(polar);
        return new Vector3(radial * Mathf.Sin(azimuth), Mathf.Cos(polar), -radial * Mathf.Cos(azimuth));
    }

    private void BuildHeadSurface(Transform parent, string name, Material material, System.Func<float, float, Vector3> surface)
    {
        const int rows = 42;
        const int columns = 64;
        var vertices = new Vector3[(rows + 1) * columns];
        var triangles = new int[rows * columns * 6];
        int index = 0;
        for (int row = 0; row <= rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int current = row * columns + col;
                vertices[current] = surface((float)row / rows, (float)col / columns * 2 * Mathf.PI - Mathf.PI);
                if (row == rows)
                    continue;
                int adjacent = row * columns + (col + 1) % columns;
                triangles[index++] = current;
                triangles[index++] = adjacent;
                triangles[index++] = current + columns;
                triangles[index++] = adjacent;
                triangles[index++] = adjacent + columns;
                triangles[index++] = current + columns;
            }
        }

        var mesh = new Mesh { name = name, vertices = vertices, triangles = triangles };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        faceMeshes.Add(mesh);
        GameObject model = Group(name, parent, Vector3.zero).gameObject;
        model.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer renderer = model.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    private void BuildOwnerHands()
    {
        Transform hands = Group("阿嬤的雙手", shopCamera.transform, Vector3.zero);
        Material sleeve = Finish(new Color32(95, 40, 43, 255));
        Material cuff = Finish(new Color32(124, 64, 64, 255));
        Material skin = SkinFinish(new Color32(199, 139, 102, 255));
        Material knuckle = SkinFinish(new Color32(184, 117, 86, 255));
        Material crease = SkinFinish(new Color32(149, 91, 68, 255));
        Material nail = SkinFinish(new Color32(221, 163, 136, 255));
        Material ageSpot = SkinFinish(new Color32(167, 108, 77, 255));

        for (int side = -1; side <= 1; side += 2)
        {
            Vector3 restingPosition = side < 0 ? new Vector3(-.29f, -.266f, .76f) : new Vector3(.315f, -.291f, .70f);
            Transform hand = Group(side < 0 ? "左手" : "右手", hands, restingPosition);
            hand.localRotation = side < 0 ? Quaternion.Euler(-4, 14, -8) : Quaternion.Euler(-14, -23, 15);
            Transform forearm = Stroke(hand, "Sleeve", new Vector3(side * .04f, -.069f, -.25f),
                new Vector3(0, -.023f, -.078f), .053f, sleeve);
            if (side > 0)
            {
                rightHand = hand;
                rightSleeve = forearm;
                rightHandRest = restingPosition;
                rightHandRestRotation = hand.localRotation;
            }
            Shape(hand, "Bound cuff", PrimitiveType.Sphere, new Vector3(0, -.023f, -.091f), new Vector3(.104f, .086f, .048f), cuff);
            Stroke(hand, "Wrist", new Vector3(0, -.019f, -.10f), new Vector3(0, -.008f, -.046f), .033f, skin);
            Shape(hand, "Palm", PrimitiveType.Sphere, Vector3.zero, new Vector3(.088f, .041f, .13f), skin);

            float[] lengths = { .078f, .087f, .079f, .062f };
            for (int finger = 0; finger < lengths.Length; finger++)
            {
                float x = side * (-.031f + finger * .020f);
                float length = lengths[finger];
                Vector3 basePoint = new Vector3(x, .001f, .045f - finger * .004f);
                Vector3 middle = basePoint + new Vector3(side * (finger - 1.5f) * .004f, .008f, length * .54f);
                Vector3 tip = middle + new Vector3(side * .002f, -.007f - finger * .003f, length * .41f);
                float radius = finger == 3 ? .008f : .0097f;
                Transform digit = Group("Finger " + (finger + 1), hand, basePoint);
                BuildFinger(digit, Vector3.zero, middle - basePoint, tip - basePoint, radius, skin, knuckle, crease, nail);
                if (side > 0)
                    gripFingers[finger] = digit;
            }
            BuildFinger(hand, new Vector3(-side * .031f, -.006f, -.027f),
                new Vector3(-side * .057f, .010f, .006f), new Vector3(-side * .065f, .005f, .039f),
                .012f, skin, knuckle, crease, nail);

            for (int i = 0; i < 4; i++)
            {
                float x = side * (-.022f + i * .013f);
                Shape(hand, "Hand tendon", PrimitiveType.Sphere, new Vector3(x, .014f, .001f),
                    new Vector3(.009f, .010f, .046f), skin).transform.localRotation = Quaternion.Euler(0, side * (i - 1.5f) * 5, 0);
                Shape(hand, "Age spot", PrimitiveType.Sphere, new Vector3(x, .019f, .016f - i * .011f),
                    new Vector3(.0045f, .002f, .004f), ageSpot);
            }
        }
    }

    private void AnimatePullHand(bool lookingAtDrawers)
    {
        float elapsed = Time.unscaledTime - pullStartedAt;
        bool pulling = lookingAtDrawers && opened >= 0 && elapsed < HandReturnDuration;
        float reach = 0;
        float grip = 0;
        Vector3 palm = rightHandRest;
        Quaternion rotation = rightHandRestRotation;
        if (pulling)
        {
            reach = Mathf.SmoothStep(0, 1, elapsed / ReachDuration)
                * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(PullDuration, HandReturnDuration, elapsed)));
            grip = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(ReachDuration * .57f, ReachDuration, elapsed))
                * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(PullDuration - .025f, PullDuration + .12f, elapsed)));
            Vector3 handle = shopCamera.transform.InverseTransformPoint(drawerHandles[opened].position);
            // Match the handle's screen position while keeping the first-person hand in the foreground.
            if (handle.z > .1f)
            {
                float depth = Mathf.Clamp(handle.z * .37f, .78f, 1.22f);
                Vector3 gripPoint = handle * (depth / handle.z);
                Quaternion gripRotation = Quaternion.Euler(13, -9, Mathf.Clamp(-handle.x * 11, -25, 25));
                Vector3 gripPalm = gripPoint - gripRotation * new Vector3(-.005f, -.018f, .079f);
                palm = Vector3.Lerp(rightHandRest, gripPalm, reach);
                rotation = Quaternion.Slerp(rightHandRestRotation, gripRotation, reach);
            }
        }

        rightHand.SetLocalPositionAndRotation(palm, rotation);
        for (int i = 0; i < gripFingers.Length; i++)
            gripFingers[i].localRotation = Quaternion.Euler(grip * (45 + i * 4), 0, 0);

        Vector3 sleeveStart = Vector3.Lerp(new Vector3(.04f, -.069f, -.25f),
            rightHand.InverseTransformPoint(shopCamera.transform.TransformPoint(new Vector3(.38f, -.45f, .49f))), reach);
        SetStroke(rightSleeve, sleeveStart, new Vector3(0, -.023f, -.078f), .053f);
    }

    private static void BuildFinger(Transform hand, Vector3 start, Vector3 joint, Vector3 tip, float radius,
        Material skin, Material knuckle, Material crease, Material nail)
    {
        Stroke(hand, "First phalanx", start, joint, radius, skin);
        Stroke(hand, "Fingertip", joint, tip, radius * .88f, skin);
        Shape(hand, "Knuckle", PrimitiveType.Sphere, joint, Vector3.one * (radius * 2.02f), knuckle);
        Shape(hand, "Flexion crease", PrimitiveType.Sphere, joint + Vector3.up * (radius * .9f),
            new Vector3(radius * 1.38f, .0018f, .003f), crease);
        Shape(hand, "Nail", PrimitiveType.Sphere, Vector3.Lerp(joint, tip, .78f) + Vector3.up * (radius * .74f),
            new Vector3(radius * 1.24f, .003f, radius * 1.65f), nail)
            .transform.localRotation = Quaternion.FromToRotation(Vector3.forward, tip - joint);
    }

    private static Transform Stroke(Transform parent, string name, Vector3 start, Vector3 end, float radius, Material material)
    {
        Transform stroke = Shape(parent, name, PrimitiveType.Capsule, Vector3.zero, Vector3.one, material).transform;
        SetStroke(stroke, start, end, radius);
        return stroke;
    }

    private static void SetStroke(Transform stroke, Vector3 start, Vector3 end, float radius)
    {
        Vector3 span = end - start;
        stroke.SetLocalPositionAndRotation((start + end) * .5f, Quaternion.FromToRotation(Vector3.up, span));
        stroke.localScale = new Vector3(radius * 2, span.magnitude * .5f + radius, radius * 2);
    }
}
